export interface User {
    username: string;
    displayName: string;
    token: string;
    refreshToken: string;
    image?: string;
}

export interface UserFormValues {
    email: string;
    password: string;
    displayName?: string;
    username?: string;
}

export interface RefreshToken {
    accessToken: string;
    refreshToken: string;
}