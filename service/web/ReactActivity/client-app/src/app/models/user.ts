export interface User {
    username: string;
    displayName: string;
    token: string;
    image?: string;
}

export interface UserFormValuesLogin {
    email: string;
    password: string;
}

export interface UserFormValuesRegister {
    email: string;
    password: string;
    displayName?: string;
    username?: string;
}

export interface UserFormValuesForgetPassword {
    email: string;
}

export interface UserFormValuesResetPassword {
    email: string;
    token: string;
    password: string;
}

export interface UserLogout {
    username: string;
    token: string;
}
