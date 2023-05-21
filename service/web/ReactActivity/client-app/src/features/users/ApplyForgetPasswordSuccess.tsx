import { Button, Header, Icon, Segment } from 'semantic-ui-react';

export default function ApplyForgetPasswordSuccess() {
    return (
        <Segment placeholder textAlign="center">
            <Header icon color="green">
                <Icon name="check" />
                Successfully submit forget password apply!
            </Header>
            <p>Please check your email (including junk mail) for the reset password</p>
        </Segment>
    );
}
