import { combineReducers } from 'redux';
import { visitorReducer } from './VisitorReducer';

export const appReducer = combineReducers( {
    visitor: visitorReducer,
})