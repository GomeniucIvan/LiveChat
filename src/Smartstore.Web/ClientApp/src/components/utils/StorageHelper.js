import { isNullOrEmpty } from "./Utils";

let widgetCompanyId = '';
let widgetVisitorId = '';

export const Storage = {
    CompanyId: function () {
        if (!isNullOrEmpty(widgetCompanyId)) {
            return widgetCompanyId;
        }

        return localStorage.getItem('companyId');
    },
    VisitorId: function () {
        return widgetVisitorId;
    },
    SetCompanyId: function (companyId) {
        widgetCompanyId = companyId;
        return;
    },
    SetVisitorId: function (visitorId) {
        widgetVisitorId = visitorId;
        return;
    }
}