import { Activity, ActivityFormValues } from '../models/activity';
import { User, UserFormValuesForgetPassword, UserFormValuesLogin, UserFormValuesResetPassword, UserLogout } from '../models/user';
import { Photo, Profile, UserActivity } from '../models/profile';
import { store } from '../stores/store';
import { router } from '../router/Routes';
import { PaginatedResult } from '../models/pagination';

import axios, { AxiosError, AxiosResponse } from 'axios';
import { toast } from 'react-toastify';
import { Logout } from '../models/logout';

const sleep = (delay: number) => {
    return new Promise((resolve) => {
        setTimeout(resolve, delay);
    });
};

axios.defaults.baseURL = process.env.REACT_APP_API_URL;
axios.defaults.withCredentials = true;

axios.interceptors.request.use((config) => {
    const token = store.commonStore.token;
    if (token && config.headers) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

axios.interceptors.response.use(
    async (response) => {
        await sleep(1000);
        const pagination = response.headers['pagination'];
        if (pagination) {
            response.data = new PaginatedResult(response.data, JSON.parse(pagination));
            return response as AxiosResponse<PaginatedResult<any>>;
        }
        return response;
    },
    (error: AxiosError) => {
        const { data, status, config, headers } = error.response as AxiosResponse;
        switch (status) {
            case 400:
                if (config.method === 'get' && data.errors !== undefined && data.errors.hasOwnProperty('id')) {
                    router.navigate('/not-found');
                }
                if (data.errors) {
                    const modalStateErrors = [];
                    for (const key in data.errors) {
                        if (data.errors[key]) {
                            modalStateErrors.push(data.errors[key]);
                        }
                    }
                    throw modalStateErrors.flat();
                } else {
                    toast.error(data);
                }
                break;
            case 401:
                //TODO: Check if the error is invalid_token and then logout.
                if (status === 401 && headers['www-authenticate']?.startsWith('Bearer error="invalid_token"'))
                {
                    store.userStore.logout();
                    toast.error('Session expired - please login again');
                }
                break;
            case 403:
                toast.error('forbidden');
                break;
            case 404:
                router.navigate('/not-found');
                break;
            case 500:
                store.commonStore.setServerError(data);
                router.navigate('/server-error');
                break;
        }
        return Promise.reject(error);
    }
);

const responseBody = <T>(response: AxiosResponse<T>) => response.data;

const requests = {
    get: <T>(url: string) => axios.get<T>(url).then(responseBody),
    post: <T>(url: string, body: {}) => axios.post<T>(url, body).then(responseBody),
    put: <T>(url: string, body: {}) => axios.put<T>(url, body).then(responseBody),
    del: <T>(url: string) => axios.delete<T>(url).then(responseBody),
};

const Activities = {
    list: (params: URLSearchParams) =>
        axios.get<PaginatedResult<Activity[]>>('/activities', { params }).then(responseBody),
    details: (id: string) => requests.get<Activity>(`/activities/${id}`),
    create: (activity: ActivityFormValues) => requests.post<void>('/activities', activity),
    update: (activity: ActivityFormValues) => requests.put<void>(`/activities/${activity.id}`, activity),
    delete: (id: string) => requests.del<void>(`/activities/${id}`),
    attend: (id: string) => requests.post<void>(`/activities/${id}/attend`, {}),
};

const Account = {
    current: () => requests.get<User>('/account'),
    login: (user: UserFormValuesLogin) => requests.post<User>(`/account/login`, user),
    register: (user: UserFormValuesLogin) => requests.post<User>(`/account/register`, user),
    fbLogin: (accessToken: string) => requests
        .post<User>(`/account/fbLogin?accessToken=${accessToken}`, {}),
    googleLogin: (accessToken: string) => requests
        .post<User>(`/account/googleLogin?accessToken=${accessToken}`, {}),
    refreshToken: () => requests.post<User>('/account/refresh-token', {}),
    logout: (user: UserLogout) => requests.post<Logout>('/account/logout', user),
    verifyEmail: (token: string, email: string) =>
        requests.post<void>(`/account/verifyEmail?token=${token}&email=${email}`, {}),
    resendEmailConfirm: (email: string) =>
        requests.get<void>(`/account/resendEmailConfirmationLink?email=${email}`),
    forgetPassword: (user: UserFormValuesForgetPassword) => requests.post<void>(`/account/forgotPassword`, user),
    resetPassword: (user: UserFormValuesResetPassword) => requests.post<void>(`/account/resetPassword`, user),
};

const Profiles = {
    get: (username: string) => requests.get<Profile>(`/profiles/${username}`),
    updateProfile: (profile: Partial<Profile>) => requests.put(`/profiles`, profile),
    uploadPhoto: (file: Blob) => {
        let formData = new FormData();
        formData.append('File', file);
        return axios.post<Photo>('photos', formData, {
            headers: { 'Content-type': 'multipart/form-data' },
        });
    },
    setMainPhoto: (id: string) => requests.post(`/photos/${id}/setMain`, {}),
    deletePhoto: (id: string) => requests.del(`/photos/${id}`),
    updateFollower: (username: string) => requests.post(`/follow/${username}`, {}),
    listFollowings: (username: string, predicate: string) =>
        requests.get<Profile[]>(`/follow/${username}?predicate=${predicate}`),
    listActivities: (username: string, predicate: string) =>
        requests.get<UserActivity[]>(`/profiles/${username}/activities?predicate=${predicate}`),
};

const agent = {
    Activities,
    Account,
    Profiles,
};

export default agent;
