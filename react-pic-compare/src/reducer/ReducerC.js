/**
 CreateDate:
 */
const reducerC = function(state = { leftUrl:"", rightUrl:"" }, action){
    switch (action.type){
        case "Change":
            return { ...state,
                leftUrl: action.payload.leftUrl,
                rightUrl: action.payload.rightUrl
            }
        default:
            return state
    }
}
export {reducerC}