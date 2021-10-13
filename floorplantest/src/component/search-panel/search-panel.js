/**
 CreateDate:
 */
import React from 'react'
import "./search-panel.css"
import store from "../../redux/store";
class SearchPanel extends React.Component {
    constructor() {
        super();
        this.state = {
            inputValue : ""
        }
    }
    render() {
        return (
            <div class="search-wrapper">
                查询房源: <input type="text" onChange={ this.onChange.bind(this) } value={this.state.inputValue}></input>
                <div onClick={ this.search.bind(this) } class="search-button">查询</div>
            </div>
        )
    }
    search(e){
        console.log("查询hid:", this.state.inputValue, e.target.value)
        if( this.state.inputValue.length > 10 ){
            store.dispatch({
                type: "Search",
                payload : {
                    hid: this.state.inputValue
                }
            })
        }
        this.setState({
            inputValue: ""
        })
    }
    onChange(e){
        console.log(e.target.value)
        this.setState({
            inputValue: e.target.value
        })
    }
}

export {SearchPanel}