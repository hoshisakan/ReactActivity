import agent from '../api/agent';
import { store } from './store';
import { User, UserFormValues } from '../models/user';
import { router } from '../router/Routes';

import { makeAutoObservable, runInAction } from 'mobx';

export default class userStore {
    user: User | null = null;

    constructor() {
        makeAutoObservable(this);
    }

    get isLoggedIn() {
        return !!this.user;
    }

    login = async (requestValues: UserFormValues) => {
        try {
            const user = await agent.Account.login(requestValues);
            store.commonStore.setToken(user.token);
            runInAction(() => (this.user = user));
            router.navigate('/activities');
            store.modalStore.closeModal();
        } catch (error) {
            throw error;
        }
    };

    register = async (requestValues: UserFormValues) => {
        try {
            const user = await agent.Account.register(requestValues);
            store.commonStore.setToken(user.token);
            runInAction(() => (this.user = user));
            router.navigate('/activities');
            store.modalStore.closeModal();
        } catch (error) {
            throw error;
        }
    };

    setImage = (image: string) => {
        if (this.user)
        {
            this.user.image = image;
        }
    };

    logout = () => {
        store.commonStore.setToken(null);
        this.user = null;
        router.navigate('/');
    };

    getUser = async () => {
        try {
            const user = await agent.Account.current();
            runInAction(() => (this.user = user));
        } catch (error) {
            console.log(error);
        }
    };
}
