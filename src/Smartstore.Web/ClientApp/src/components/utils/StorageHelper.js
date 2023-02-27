import { isNullOrEmpty } from "./Utils";

let widgetCompanyId = '';
let widgetVisitorId = '';

export const Storage = {
    get CompanyId() {
        if (!isNullOrEmpty(widgetCompanyId)) {
            return widgetCompanyId;
        }

        return localStorage.getItem('companyId');
    },
    get VisitorId() {
        return widgetVisitorId;
    },
    SetCompanyId: (companyId) => {
        widgetCompanyId = companyId;
        return;
    },
    SetVisitorId: (visitorId) => {
        widgetVisitorId = visitorId;
        return;
    }
}