import $ from 'jquery'
import { Storage } from './StorageHelper';
import { isNullOrEmpty, redirectToLogin } from './Utils';
const prefixRoute = "/odata/v1/"

export const get = async (sufixRoute, location) => {
    const url = `${prefixRoute}${sufixRoute}`;
    const response = await fetch(url, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': localStorage.getItem('access_token'),
            'RefreshToken': localStorage.getItem('refresh_token'),
            'PathName': isNullOrEmpty(location) ? '' : location.pathname,
        },
        credentials: 'include'
    });


    const jsonData = await response.json();

    redirectToLogin(/*location*/ location, /*response*/ jsonData, /*forceRedirect*/ null);
    return jsonData;
};

export const getNotAsync = (sufixRoute, location) => {
    const url = `${prefixRoute}${sufixRoute}`;

    const response = fetch(url, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': localStorage.getItem('access_token'),
            'RefreshToken': localStorage.getItem('refresh_token'),
        },
        credentials: 'include'
    });

    const jsonData = response.json();
    redirectToLogin(/*location*/ location, /*response*/ jsonData, /*forceRedirect*/ null);
    return jsonData;
};

export const post = async (sufixRoute, object, location) => {
    const url = `${prefixRoute}${sufixRoute}`;
    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': localStorage.getItem('access_token'),
            'RefreshToken': localStorage.getItem('refresh_token'),
            'PathName': isNullOrEmpty(location) ? '' : location.pathname,
        },
        credentials: 'include',
        body: JSON.stringify(object)
    });

    const jsonData = await response.json();
    redirectToLogin(/*location*/ location, /*response*/ jsonData, /*forceRedirect*/ null);
    return jsonData;
};

export const postLauncher = async (sufixRoute, visitorId, object, location, isInitCall) => {
    const url = `${prefixRoute}${sufixRoute}`;

    const response = await fetch(url, {
        method: 'POST',
        headers: {
            "Content-Type": "application/json",
            'Authorization': localStorage.getItem('access_token'),
            'RefreshToken': localStorage.getItem('refresh_token'),
            "VisitorId": visitorId, 
            'PathName': isNullOrEmpty(location) ? '' : location.pathname,
        },
        //credentials: 'include',
        body: JSON.stringify(object)
    });

    const jsonData = await response.json();
    redirectToLogin(/*location*/ location, /*response*/ jsonData, /*forceRedirect*/ null);
    return jsonData;
};

export const postChat = async (sufixRoute, visitorId, object, isInitCall) => {
    const url = `${prefixRoute}chat/${sufixRoute}`;
    const response = await fetch(url, {
        method: 'POST',
        headers: {
            "Content-Type": "application/json",
            "InitAuthorization": isInitCall ? JSON.stringify(object) : null,
            "CompanyId": Storage.CompanyId,
            "VisitorId": visitorId,
        },
        //credentials: 'include',
        body: JSON.stringify(object)
    });

    const jsonData = await response.json();

    return jsonData;
};

export const postData = async (url, object) => {
    return new Promise(function (resolve, reject) {
        $.ajax({
            url: url,
            type: "POST",
            headers: {
                'Authorization': localStorage.getItem('access_token'),
                'RefreshToken': localStorage.getItem('refresh_token'),
            },
            data: object,
            beforeSend: function () {
            },
            success: function (response) {
                return resolve($.parseJSON(response)) // Resolve promise and when success
            },
            error: function (err) {
                reject(err) // Reject the promise and go to catch()
            }
        });
    });
};