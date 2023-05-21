import { useStore } from '../../app/stores/store';
import LoginForm from '../users/LoginForm';

import { Link } from 'react-router-dom';
import { Container, Header, Segment, Image, Button, Divider } from 'semantic-ui-react';
import { observer } from 'mobx-react-lite';
import RegisterForm from '../users/RegisterForm';
import FacebookLogin from '@greatsumini/react-facebook-login';
// import ForgetPassword from '../users/ForgetPassword';

export default observer(function HomePage() {
    const { userStore, modalStore } = useStore();

    return (
        <Segment inverted textAlign="center" vertical className="masthead">
            <Container text>
                <Header as="h1">
                    <Image size="massive" src="/assets/logo.png" alt="logo" style={{ marginBottom: 12 }} />
                    Reactivities
                </Header>
                {userStore.isLoggedIn ? (
                    <>
                        <Header as="h2" inverted>
                            Welcome back {userStore.user?.displayName}
                        </Header>
                        <Button as={Link} to="/activities" size="huge" inverted>
                            Go to Activities!
                        </Button>
                    </>
                ) : (
                    <>
                        <Button onClick={() => modalStore.openModal(<LoginForm />)} to="/login" size="huge" inverted>
                            Login!
                        </Button>
                        <Button onClick={() => modalStore.openModal(<RegisterForm />)} size="huge" inverted>
                            Register!
                        </Button>
                        <Divider hidden></Divider>
                        <Link to={'account/forgetPassword'} style={{ textDecoration: 'underline', color: 'white' }}>Forget Password?</Link>
                        {/* <Button
                            onClick={() => modalStore.openModal(<ForgetPassword />)}
                            style={{
                                textDecoration: 'underline',
                                boxShadow: 'none',
                                backgroundColor: 'transparent',
                                borderBottom: 'none',
                                position: 'relative',
                            }}
                            size="small"
                            inverted
                        >
                            Forget Password?
                        </Button> */}
                        <Divider horizontal inverted>
                            Or
                        </Divider>
                        <Button
                            as={FacebookLogin}
                            appId={process.env.REACT_APP_FACEBOOK_APP_ID!}
                            size="huge"
                            inverted
                            color="facebook"
                            content="Login with Facebook"
                            loading={userStore.fbLoading}
                            onSuccess={(response: any) => {
                                // console.log('Login success', response);
                                userStore.facebookLogin(response.accessToken);
                            }}
                            onFail={(error: any) => {
                                console.log('Login fail', error);
                            }}
                        />
                    </>
                )}
            </Container>
        </Segment>
    );
});
