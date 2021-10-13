import {createStore, combineReducers} from "redux";
import {reducerC} from "../reducer/ReducerC";
import {reducerD} from "../reducer/ReducerD";

const cr = combineReducers({
    pagePair: reducerC,
    search: reducerD
})
const store = createStore(cr);
export default store;