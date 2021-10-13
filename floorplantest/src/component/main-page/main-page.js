/**
 CreateDate:20210605
 */
import React from "react"
import "./main-page.css"
import {Fetcher} from "../util/Fecther";
import {Util} from "../util/Util";
import store from "../../redux/store";
import {PicPanel} from "../pic-panel/pic-panel";
import {SearchPanel} from "../search-panel/search-panel";

class MainPage extends React.Component{
    state = {
        currHouse: {},
        houseNum: 0,
        currHouseImageNum: 0,
        currIndex: 0, //当前房源照片中的索引
        currHouseIndex: 0,
        currPackageId : "",
        currFloorPlan : "",
        currBucket : "",
        currImages:[],
        rootPath: "https://vrhouse-storage.oss-cn-shanghai.aliyuncs.com/floorplantestdata/",
        defaultPath : "HouseInfo.json",
        jsonPath: "",
        houseInfos : [],
        currImageNum:0
    }
    componentDidMount() {
        console.log("componentDidMount:\n",store.getState())
        let path = Util.getUrlParameter("path");
        console.log(path)
        if(!path) path = this.state.defaultPath;
        if(path && !path.endsWith(".json")){
            window.alert('当前路径不合理，请确认。\n将加载默认数据')
            path = this.state.defaultPath;
        }
        this.setState({
            jsonPath : this.state.rootPath + path
        }, this.initData.bind(this))

        store.subscribe(() => {
            console.log('当前store',store.getState(), )
            let searchHid = store.getState().search.inputHid;
            this.checkSearchValid(searchHid)
        })
    }
    checkSearchValid(searchHid){
        if( !searchHid ) return;

        let c = this.state.houseInfos.map( h => h.PackageId).find(p => p.includes(searchHid));
        console.log("查询结果:", c)
        console.log("当前hid:", this.state.currPackageId)
        if( c ){

            let index = this.state.houseInfos.map( h => h.PackageId ).indexOf(c);
            console.log('index:', index)
            let nextHouse = this.state.houseInfos[index]
            this.getHouseInfo(nextHouse);
            this.setState({
                currHouseIndex: index,
                currPackageId: c,
                currIndex: 0
            })

            //需要将查询字段置空，否则会无限循环
            store.dispatch({
                type: "Search",
                payload : {
                    hid: ""
                }
            })
        }
    }
    initData(){
        //请求所有房源数据
        Fetcher.getData(this.state.jsonPath+ "?" + Util.UUID8Bit()).then((data) =>{
            this.setState({
                houseInfos: JSON.parse(data)
            })
            this.setState({
                currHouse: this.state.houseInfos[0],
                houseNum: this.state.houseInfos.length,
                currPackageId: this.state.houseInfos[0].PackageId,
                currBucket: this.state.houseInfos[0].Bucket
            })
            //请求第一个房源的图片地址
            this.getHouseInfo(this.state.houseInfos[0])
        }).catch((e)=>{
            console.log(e)
            alert('数据加载失败');
        })
    }
    render(){
        const options = this.state.houseInfos.map( h => (<option key={h.PackageId} value={h.PackageId}>{h.PackageId}</option>) )
        return(
            <div>
                <div className="id_section">
                    <div className="id_title">
                        <div className="id_s1">当前房源：</div>
                        <div className="id_path">{this.state.currPackageId}</div>
                        <div>，房源总量：</div>
                        <div className="id_path">{this.state.houseNum}</div>
                        <div>，当前浏览：</div>
                        <div className="id_path">{this.state.currHouseIndex + 1} - {this.state.houseNum}</div>
                    </div>
                    <div className="id_title">
                        <div className="id_s2">当前户型图：</div>
                        <div className="id_path">{this.state.currFloorPlan}</div>
                        <div className="id_num">，当前房源照片数量：</div>
                        <div className="id_path">{this.state.currImageNum}</div>
                        <div>，当前浏览：</div>
                        <div className="id_path">{this.state.currIndex + 1} - {this.state.currImageNum}</div>
                    </div>
                    <form className="id_select">
                        <label>选择房源: </label>
                        <select onChange={this.handleSelect.bind(this)} value={this.state.currPackageId}>
                            {options}
                        </select>
                    </form>
                </div>
                <div className="header_button">
                    <button value="0" className="green" onClick={this.handleClickBeforeHouse}> 上一套 </button>
                    <button value="1" className="green" onClick={this.handleClickNextHouse}> 下一套 </button>
                    <button value="0" className='blue' onClick={this.handleClickBefore}> 上一张 </button>
                    <button value="1" className='blue'onClick={this.handleClickNext}> 下一张 </button>
                </div>
                <SearchPanel></SearchPanel>
                <PicPanel></PicPanel>
            </div>
        )
    }
    //设置显示的图片
    setUrls(){
        let index = this.state.currIndex;
        store.dispatch({
            type: "Change",
            payload: {
                leftUrl : "http://" + this.state.currHouse.Domain + "/" +this.state.currHouse.PackageId + "/FloorPlans/" + this.state.currImages[index] + "?" + Util.UUID8Bit(),
                rightUrl : "http://" + this.state.currHouse.Domain + "/" +this.state.currHouse.PackageId + "/FloorPlansTest/" + this.state.currImages[index]+ "?" + Util.UUID8Bit()
            }
        })
        this.setState({
            currFloorPlan: this.state.currImages[index],
            currImageNum: this.state.currImages.length
        })
    }

    getHouseInfo(houseInfo){
        Fetcher.getImagePath(houseInfo).then( (info) => {
            console.log('info', info)
            if(!info.floorPlan.length) alert('该房源图片为空')
            this.setState({
                currHouse: houseInfo,
                currImages: info.floorPlan,
                currHouseImageNum: info.floorPlan.length
            }, this.setUrls.bind(this));
        })
    }
    //选择另一套房源
    handleSelect(event){
        let selectIndex = event.target.options.selectedIndex;
        let selectValue = event.target.options[selectIndex].value;
        console.log(selectIndex, selectValue)
        this.setState({
            currPackageId: selectValue,
            currIndex:0,
            currHouseIndex: selectIndex
        })
        let currInfo = this.state.houseInfos.find(h => h.PackageId === selectValue);
        console.log(currInfo)
        this.getHouseInfo(currInfo);
    }
    getCurrIndex(){
        let currIndex = this.state.houseInfos.indexOf(this.state.currHouse)
        console.log('currIndex' ,currIndex)
        return typeof currIndex === 'number' ? currIndex : 0
    }
    handleClickBeforeHouse = (event) => {
        let currIndex = this.getCurrIndex();
        if( !currIndex ) {
            alert('当前已经是第一套房源')
            return
        }
        let beforeHouse = this.state.houseInfos[currIndex - 1]
        this.getHouseInfo(beforeHouse);
        this.setState({
            currHouseIndex: this.state.currHouseIndex - 1,
            currPackageId:beforeHouse.PackageId,
            currIndex: 0
        })
    }
    handleClickNextHouse = (event) => {
        let currIndex = this.getCurrIndex();
        if(currIndex === this.state.houseInfos.length - 1) {
            alert('当前已经是最后一套房源')
            return
        }
        let nextHouse = this.state.houseInfos[currIndex + 1]
        this.getHouseInfo(nextHouse);
        this.setState({
            currHouseIndex: this.state.currHouseIndex + 1,
            currPackageId : nextHouse.PackageId,
            currIndex: 0
        })
    }
    handleClickNext = (event) => {
        if(this.state.currIndex === this.state.currImages.length - 1) {
            console.log('已经是最后一张')
            alert('已经是最后一张')
            return
        }
        this.setState({
            similarRatio: "...",
            currIndex : this.state.currIndex + 1
        }, this.setUrls.bind(this))
    }
    handleClickBefore = (event) => {
        if(!this.state.currIndex) {
            console.log('已经是第一张')
            alert('已经是第一张')
            return
        }
        this.setState({
            similarRatio: "...",
            currIndex : this.state.currIndex - 1
        }, this.setUrls.bind(this));
    }
}

export {MainPage}