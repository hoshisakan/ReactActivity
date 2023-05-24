import { Formik, Form } from 'formik';
import { Button, Header, Segment } from 'semantic-ui-react';
import * as Yup from 'yup';
import { useStore } from '../../app/stores/store';
import { observer } from 'mobx-react-lite';
import MyTextInput from '../../app/common/form/MyTextInput';
import MyTextArea from '../../app/common/form/MyTextArea';
import { Profile } from '../../app/models/profile';

interface Props {
    setEditMode: (editMode: boolean) => void;
}

export default observer(function ProfileEditForm({ setEditMode }: Props) {
    const {
        profileStore: { profile, updateProfile },
    } = useStore();

    const validationSchema = Yup.object({
        displayName: Yup.string().required('The display name is required'),
    });
    const initialValues = {
        displayName: profile?.displayName,
        bio: profile?.bio,
    };

    function handleFormSubmit(values: Partial<Profile>) {
        updateProfile(values).then(() => setEditMode(false));
    }

    return (
        <Segment clearing>
            <Header content="About Display Name" sub color="teal" />
            <Formik
                validationSchema={validationSchema}
                initialValues={initialValues}
                onSubmit={(values) => handleFormSubmit(values)}
            >
                {({ handleSubmit, isValid, isSubmitting, dirty }) => (
                    <Form className="ui form" onSubmit={handleSubmit} autoComplete="off">
                        <MyTextInput name="displayName" placeholder="Display Name" />
                        <MyTextArea rows={3} placeholder="Add your bio" name="bio" />
                        <Button
                            loading={isSubmitting}
                            floated="right"
                            positive
                            type="submit"
                            content="Update profile"
                            disabled={!dirty || !isValid}
                        />
                    </Form>
                )}
            </Formik>
        </Segment>
    );
});
