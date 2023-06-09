import ServerError from '../models/serverError';

import { makeAutoObservable, reaction } from 'mobx';

export default class CommonStore {
    error: ServerError | null = null;
    token: string | null = localStorage.getItem('access_token');
    appLoaded = false;
    windowSize = {
        width: window.innerWidth,
        height: window.innerHeight
    };
    detectedMobileDevice = false;

    constructor() {
        makeAutoObservable(this);

        reaction(
            () => this.token,
            (token) => {
                if (token) {
                    localStorage.setItem('access_token', token);
                } else {
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

    setAppLoaded = () => {
        this.appLoaded = true;
    };

    setDetectedMobileDevice = () => {
        if (this.windowSize.width >= 400)
        {
            this.detectedMobileDevice = false;
        }
        else if (this.windowSize.width < 400)
        {
            this.detectedMobileDevice = true;
        }
    }
}
