import { useStore } from '../../app/stores/store';
import LoginForm from '../users/LoginForm';
import RegisterForm from '../users/RegisterForm';

import { Link } from 'react-router-dom';
import { Container, Header, Segment, Image, Button, Divider } from 'semantic-ui-react';
import { observer } from 'mobx-react-lite';
import { GoogleLogin } from '@leecheuk/react-google-login';
import { gapi } from 'gapi-script';

export default observer(function HomePage() {
    const { userStore, modalStore } = useStore();

    gapi.load('client:auth2', () => {
        gapi.client.init({
            clientId: 'your client id will be display here',
            plugin_name: 'chat',
        });
    });

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
                        <Link to={'account/forgetPassword'} style={{ textDecoration: 'underline', color: 'white' }}>
                            Forget Password?
                        </Link>
                        <Divider horizontal inverted>
                            Or
                        </Divider>
                        <Button
                            as={GoogleLogin}
                            clientId={process.env.REACT_APP_GOOGLE_CLIENT_ID!}
                            size="huge"
                            inverted
                            color="google plus"
                            content="Login with Google"
                            loading={userStore.googleLoading}
                            onSuccess={(response: any) => {
                                // console.log('Login success', response);
                                // console.log('Login success', response.accessToken);
                                userStore.googleLogin(response.accessToken);
                            }}
                            onFail={(error: any) => {
                                // console.log('Login fail', error);
                                console.log(error);
                            }}
                        />
                    </>
                )}
            </Container>
        </Segment>
    );
});
