import agent from '../../app/api/agent';
import useQuery from '../../app/util/hooks';
import LoginForm from './LoginForm';
import { useStore } from '../../app/stores/store';

import React, { useEffect } from 'react';
import { toast } from 'react-toastify';
import { Button, Header, Icon, Segment } from 'semantic-ui-react';


export default function ConfirmEmail() {
    const { modalStore } = useStore();
    const email = useQuery().get('email') as string;
    const token = useQuery().get('token') as string;

    const Status = {
        Verifying: 'Verifying',
        Failed: 'Failed',
        Success: 'Success',
    };

    const [status, setStatus] = React.useState(Status.Verifying);

    function handleConfirmEmailResend() {
        agent.Account.resendEmailConfirm(email)
            .then(() => {
                toast.success('Verification email resent - please check your email');
            })
            .catch((error) => console.log(error));
    }

    useEffect(() => {
        agent.Account.verifyEmail(token, email)
            .then(() => {
                setStatus(Status.Success);
            })
            .catch(() => {
                setStatus(Status.Failed);
            });
    }, [Status.Failed, Status.Success, email, token]);

    function getBody() {
        switch (status) {
            case Status.Verifying:
                return <p>Verifying. . .</p>;
            case Status.Failed:
                return (
                    <div className="center">
                        <p>Verification failed. You can try resending the verification email</p>
                        <Button primary onClick={handleConfirmEmailResend} content="Resend email" size="huge" />
                    </div>
                );
            case Status.Success:
                return (
                    <div className="center">
                        <p>Email has been verified. You can now login</p>
                        <Button
                            primary
                            onClick={() => modalStore.openModal(<LoginForm />)}
                            content="Login"
                            size="huge"
                        />
                    </div>
                );
        }
    }

    return (
        <Segment placeholder textAlign="center">
            <Header>
                <Icon name="envelope" />
                Email verification
            </Header>
            <Segment.Inline>{getBody()}</Segment.Inline>
        </Segment>
    );
}
