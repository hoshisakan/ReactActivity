import agent from '../api/agent';
import { store } from './store';
import {
    User,
    UserFormValuesForgetPassword,
    UserFormValuesLogin,
    UserFormValuesRegister,
    UserFormValuesResetPassword,
    UserLogout,
} from '../models/user';
import { router } from '../router/Routes';
import { Buffer } from 'buffer';
import { makeAutoObservable, runInAction } from 'mobx';

export default class userStore {
    user: User | null = null;
    fbLoading = false;
    googleLoading = false;
    refreshTokenTimeout: any;
    allowLogout = false;

    constructor() {
        makeAutoObservable(this);
    }

    get isLoggedIn() {
        return !!this.user;
    }

    login = async (requestValues: UserFormValuesLogin) => {
        try {
            const user = await agent.Account.login(requestValues);
            store.commonStore.setToken(user.token);
            this.startRefreshTokenTimer(user);
            runInAction(() => (this.user = user));
            router.navigate('/activities');
            store.modalStore.closeModal();
        } catch (error) {
            throw error;
        }
    };

    register = async (requestValues: UserFormValuesRegister) => {
        try {
            await agent.Account.register(requestValues);
            router.navigate(`/account/registerSuccess?email=${requestValues.email}`);
            store.modalStore.closeModal();
        } catch (error: any) {
            if (error?.response?.status === 400) {
                throw error;
            }
            store.modalStore.closeModal();
            console.log(500);
        }
    };

    forgetPassword = async (requestValues: UserFormValuesForgetPassword) => {
        try {
            console.log(requestValues);
            await agent.Account.forgetPassword(requestValues);
            router.navigate(`/account/applyForgetPasswordSuccess?email=${requestValues?.email}`);
            console.log('success');
            // store.modalStore.closeModal();
        } catch (error: any) {
            if (error?.response?.status === 400) {
                throw error;
            }
            // store.modalStore.closeModal();
            // console.log(500);
            console.log(error);
        }
    };

    resetPassword = async (requestValues: UserFormValuesResetPassword) => {
        try {
            await agent.Account.resetPassword(requestValues);
            router.navigate(`/account/resetPasswordSuccess?email=${requestValues?.email}`);
            // store.modalStore.closeModal();
        } catch (error: any) {
            if (error?.response?.status === 400) {
                throw error;
            }
            // store.modalStore.closeModal();
            // console.log(500);
            console.log(error);
        }
    };

    setImage = (image: string) => {
        if (this.user) {
            this.user.image = image;
        }
    };

    logout = async () => {
        try {
            const userLogout: UserLogout = {
                username: this.user!.username,
                token: this.user!.token,
            };
            const response = await agent.Account.logout(userLogout);

            runInAction(() => {
                if (response.isLogout) {
                    this.allowLogout = true;
                } else {
                    this.allowLogout = false;
                }
                this.user = null;
                store.commonStore.setToken(null);
                window.localStorage.removeItem('access_token');
            });
            if (this.allowLogout) {
                router.navigate('/');
            }
        } catch (error) {
            throw error;
        }
    };

    getUser = async () => {
        try {
            const user = await agent.Account.current();
            store.commonStore.setToken(user.token);
            runInAction(() => (this.user = user));
            this.startRefreshTokenTimer(user);
        } catch (error) {
            console.log(error);
        }
    };

    setDisplayName = (displayName: string) => {
        if (this.user) {
            this.user.displayName = displayName;
        }
    };

    facebookLogin = async (accessToken: string) => {
        try {
            this.fbLoading = true;
            const user = await agent.Account.fbLogin(accessToken);
            store.commonStore.setToken(user.token);
            this.startRefreshTokenTimer(user);
            runInAction(() => {
                this.user = user;
                this.fbLoading = false;
            });
            router.navigate('/activities');
        } catch (error) {
            console.log(error);
            runInAction(() => (this.fbLoading = false));
        }
    };

    googleLogin = async (accessToken: string) => {
        try {
            this.googleLoading = true;
            const user = await agent.Account.googleLogin(accessToken);
            store.commonStore.setToken(user.token);
            this.startRefreshTokenTimer(user);
            runInAction(() => {
                this.user = user;
                this.googleLoading = false;
            });
            router.navigate('/activities');
        } catch (error) {
            console.log(error);
            runInAction(() => (this.googleLoading = false));
        }
    };

    refreshToken = async () => {
        this.stopRefreshTokenTimer();
        try {
            const user = await agent.Account.refreshToken();
            runInAction(() => (this.user = user));
            store.commonStore.setToken(user.token);
            this.startRefreshTokenTimer(user);
        } catch (error) {
            console.log(error);
        }
    };

    private startRefreshTokenTimer(user: User) {
        try {
            const pastToken = user.token.split('.')[1];
            const jwtToken = JSON.parse(Buffer.from(pastToken, 'base64').toString('ascii'));
            // console.log(jwtToken);
            const expires = new Date(jwtToken.exp * 1000);
            // console.log(expires.getTime());
            // console.log(Date.now().toString());
            //TODO: Reduce 30 seconds from set timeout every 30 seconds refresh token is called
            const timeout = expires.getTime() - Date.now() - 30 * 1000;
            // console.log(timeout);
            //TODO: Set timeout to 30 seconds before token expires
            this.refreshTokenTimeout = setTimeout(this.refreshToken, timeout);
            // console.log(this.refreshTokenTimeout);
        } catch (error) {
            console.log(error);
        }
    }

    private stopRefreshTokenTimer() {
        clearTimeout(this.refreshTokenTimeout);
    }
}
