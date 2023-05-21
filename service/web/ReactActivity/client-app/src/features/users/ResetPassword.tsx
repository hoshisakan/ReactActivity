import MyTextInput from '../../app/common/form/MyTextInput';
import useQuery from '../../app/util/hooks';
import { useStore } from '../../app/stores/store';

import { ErrorMessage, Form, Formik } from 'formik';
import { Button, Header, Label, Segment } from 'semantic-ui-react';
import * as Yup from 'yup';

export default function ResetPassword() {
    const { userStore } = useStore();
    const email = useQuery().get('email') as string;
    const token = useQuery().get('token') as string;

    const validationSchema = Yup.object({
        email: Yup.string().required().email(),
        token: Yup.string().required(),
        password: Yup.string().required(),
    });

    return (
        <Segment placeholder textAlign="center">
            <Formik
                initialValues={{ email: email, token: token, password: '', error: null }}
                onSubmit={(values, { setErrors }) =>
                    userStore.resetPassword(values).catch((error) => setErrors({ error: error.response.data }))
                }
                validationSchema={validationSchema}
            >
                {({ handleSubmit, isSubmitting, errors }) => (
                    <Form className="ui form" onSubmit={handleSubmit} autoComplete="off">
                        <Header as="h2" content="Reset Password" color="black" textAlign="center" />
                        <MyTextInput placeholder="Email" name="email" readOnly />
                        <MyTextInput placeholder="Password" name="password" type="password" />
                        <ErrorMessage
                            name="error"
                            render={() => (
                                <Label style={{ marginBottom: 10 }} basic color="red" content={errors.error} />
                            )}
                        />
                        <Button loading={isSubmitting} positive content="Submit" type="submit" fluid />
                    </Form>
                )}
            </Formik>
        </Segment>
    );
}
