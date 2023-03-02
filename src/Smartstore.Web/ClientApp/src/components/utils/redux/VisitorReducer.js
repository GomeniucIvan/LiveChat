import { VISITOR_TYPING, VISITOR_ID } from "./AppReducer.Types";

const initialState = {
    visitor: null,
    loaded: false,
    visitorId: null
}

export const visitorReducer = (state = initialState, action) => {
    switch (action.type) {
        case VISITOR_TYPING:
            return { ...state, visitor: action.payload, loaded: true }
        case VISITOR_ID:
            return { ...state, visitorId: action.payload, loaded: true }
        default:
            return state
    }
}