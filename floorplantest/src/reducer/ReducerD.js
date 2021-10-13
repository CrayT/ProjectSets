/**
 CreateDate:
 */
const reducerD = function(state = { inputHid : "" }, action){
    switch (action.type){
        case "Search":
            return { ...state,
                inputHid: action.payload.hid
            }
        default:
            return state
    }
}
export {reducerD}