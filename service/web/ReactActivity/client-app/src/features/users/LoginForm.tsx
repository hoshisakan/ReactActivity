import MyTextInput from '../../app/common/form/MyTextInput';
import { useStore } from '../../app/stores/store';

import { ErrorMessage, Form, Formik } from 'formik';
import { Button, Header, Label } from 'semantic-ui-react';
import * as Yup from 'yup';
import { observer } from 'mobx-react-lite';


export default observer(function LoginForm() {
    const { userStore } = useStore();
    const validationSchema = Yup.object({
        email: Yup.string().required().email(),
        password: Yup.string().required(),
    });

    return (
        <Formik
            initialValues={{ email: '', password: '', error: null }}
            onSubmit={(values, { setErrors }) =>
                userStore.login(values).catch((error) => setErrors({ error: error.response.data }))
            }
            validationSchema={validationSchema}
        >
            {({ handleSubmit, isSubmitting, errors }) => (
                <Form className="ui form" onSubmit={handleSubmit} autoComplete="off">
                    <Header as="h2" content="Login to ReactActivities" color="teal" textAlign="center" />
                    <MyTextInput placeholder="Email" name="email" />
                    <MyTextInput placeholder="Password" name="password" type="password" />
                    <ErrorMessage
                        name="error"
                        render={() => <Label style={{ marginBottom: 10 }} basic color="red" content={errors.error} />}
                    />
                    <Button loading={isSubmitting} positive content="Login" type="submit" fluid />
                </Form>
            )}
        </Formik>
    );
});
