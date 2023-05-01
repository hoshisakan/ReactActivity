import ServerError from '../models/serverError';

import { makeAutoObservable, reaction } from 'mobx';

export default class CommonStore {
    error: ServerError | null = null;
    token: string | null = localStorage.getItem('access_token');
    refreshToken: string | null = localStorage.getItem('refresh_token');
    appLoaded = false;

    constructor() {
        makeAutoObservable(this);

        reaction(
            () => this.token,
            (token) => {
                if (token)
                {
                    localStorage.setItem('access_token', token);
                }
                else
                {
                    localStorage.removeItem('access_token');
                }
            }
        );
    }

    setServerError(error: ServerError) {
        this.error = error;
    }

    setToken = (token: string | null) => {
        if (token) {
            localStorage.setItem('access_token', token);
        }
        this.token = token;
    };

    get isExistAccessToken() {
        return !!this.token;
    }

    setRefreshToken = (refreshToken: string | null) => {
        if (refreshToken) {
            localStorage.setItem('refresh_token', refreshToken);
        }
        this.refreshToken = refreshToken;
    };

    get isExistRefreshToken() {
        return !!this.refreshToken;
    }

    clearLocalStorageToken = () => {
        localStorage.removeItem('access_token');
        localStorage.removeItem('refresh_token');
    };

    setAppLoaded = () => {
        this.appLoaded = true;
    };
}
