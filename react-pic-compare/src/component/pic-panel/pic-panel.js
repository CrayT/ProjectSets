/**
 CreateDate:
 */
import React from "react"
import store from "../../redux/store";
import {Util} from "../util/Util";
import "./pic-panel.css"

class PicPanel extends React.Component{
    state = {
        similarRatio:0
    }
    constructor() {
        super();
        this.state = {similarRatio:0, url: store.getState().pagePair }
    }
    render() {
        return(
            <div>
                <div className="pic_box">
                    <div className="left_part">
                        <img className='pic_size' alt="" src={this.state.url.leftUrl}></img>
                        <div>旧</div>
                    </div>
                    <div className="right_part">
                        <img  className='pic_size' alt="" src={this.state.url.rightUrl}></img>
                        <div>新</div>
                    </div>
                </div>
                <div className="bottom_section">
                    <div className="word_color">当前图片相似度：</div>
                    <h2>{this.state.similarRatio}</h2>
                </div>
            </div>
        )
    }
    componentDidMount() {
        store.subscribe(() => {
            console.log('update')
            this.setState({
                url: store.getState().pagePair
            },  this.updateSimilarRatio.bind(this))
        })
    }
    async updateSimilarRatio(){
        Util.compareImages(this.state.url.leftUrl + Util.UUID8Bit(), this.state.url.rightUrl + Util.UUID8Bit()).then( res => {
            console.log('当前比对结果：', res)
            this.setState({
                similarRatio : (res * 100).toFixed(2) + "%"
            })
        })
    }
}

export {PicPanel}