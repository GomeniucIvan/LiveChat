import { isNullOrEmpty } from "./Utils";

var widgetCompanyId = '';

export const Storage = {
    get CompanyId() {
        if (!isNullOrEmpty(widgetCompanyId)) {
            return widgetCompanyId;
        }

        return localStorage.getItem('companyId');
    },
    SetCompanyId: (companyId) => {
        widgetCompanyId = companyId;
        return;
    }
}