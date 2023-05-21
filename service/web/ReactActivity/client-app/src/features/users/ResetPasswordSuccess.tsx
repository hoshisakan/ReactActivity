import LoginForm from './LoginForm';
import { useStore } from '../../app/stores/store';

import { Button, Header, Icon, Segment } from 'semantic-ui-react';

export default function ResetPasswordSuccess() {
    const { modalStore } = useStore();

    return (
        <Segment placeholder textAlign="center">
            <Header icon color="green">
                <Icon name="check" />
                Successfully reset password!
            </Header>
            <div className="center">
                <p>Reset user password completed. You can now login</p>
                <Button primary onClick={() => modalStore.openModal(<LoginForm />)} content="Login" size="huge" />
            </div>
        </Segment>
    );
}
