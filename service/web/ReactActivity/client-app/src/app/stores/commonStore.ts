import ServerError from '../models/serverError';

import { makeAutoObservable, reaction } from 'mobx';

export default class CommonStore {
    error: ServerError | null = null;
    token: string | null = localStorage.getItem('access_token');
    refreshToken: string | null = localStorage.getItem('refresh_token');
    appLoaded = false;

    constructor() {
        makeAutoObservable(this);

        const userTokenMap = {token: this.token, refreshToken: this.refreshToken};

        reaction(
            () => userTokenMap,
            (userToken) => {
                if (userToken.token)
                {
                    localStorage.setItem('access_token', userToken.token);
                    console.log('Access token changed');
                }
                else
                {
                    localStorage.removeItem('access_token');
                    console.log('Access token removed');
                }

                if (userToken.refreshToken)
                {
                    localStorage.setItem('refresh_token', userToken.refreshToken);
                    console.log('Refresh token changed');
                }
                else
                {
                    localStorage.removeItem('refresh_token');
                    console.log('Refresh token removed');
                }
            },
        )
    }

    setServerError(error: ServerError) {
        this.error = error;
    }

    setToken = (token: string | null) => {
        if (token)
        {
            localStorage.setItem('access_token', token);
        }
        this.token = token;
    }

    setRefreshToken = (refreshToken: string | null) => {
        if (refreshToken)
        {
            localStorage.setItem('refresh_token', refreshToken);
        }
        this.refreshToken = refreshToken;
    }

    clearLocalStorageToken = () => {
        localStorage.removeItem('access_token');
        localStorage.removeItem('refresh_token');
    }

    setAppLoaded = () => {
        this.appLoaded = true;
    }
}
