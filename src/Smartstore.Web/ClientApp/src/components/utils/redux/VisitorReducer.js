import { VISITOR_TYPING } from "./AppReducer.Types";

const initialState = {
    visitor: null,
    loaded: false
}

export const visitorReducer = (state = initialState, action) => {
    switch (action.type) {
        case VISITOR_TYPING:
            return { ...state, visitor: action.payload, loaded: true }
        default:
            return state
    }
}