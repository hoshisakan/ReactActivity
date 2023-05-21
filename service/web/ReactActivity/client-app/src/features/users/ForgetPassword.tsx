import MyTextInput from '../../app/common/form/MyTextInput';
import { useStore } from '../../app/stores/store';

import { Button, Header, Label, Segment } from 'semantic-ui-react';
import { ErrorMessage, Form, Formik } from 'formik';
import * as Yup from 'yup';
import { observer } from 'mobx-react-lite';

export default observer(function ForgetPassword() {
    const { userStore } = useStore();
    const validationSchema = Yup.object({
        email: Yup.string().required().email(),
    });

    return (
        <Segment placeholder textAlign='center'>
            <Formik
                initialValues={{ email: '', error: null }}
                onSubmit={(values, { setErrors }) =>
                    userStore.forgetPassword(values).catch((error) => setErrors({ error: error.response.data }))
                }
                validationSchema={validationSchema}
            >
                {({ handleSubmit, isSubmitting, errors }) => (
                    <Form className="ui form" onSubmit={handleSubmit} autoComplete="off">
                        <Header as="h2" content="Forget Password" color="black" textAlign="center" />
                        <MyTextInput placeholder="Email" name="email" />
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
});
