import { useStore } from '../../../app/stores/store';

import { observer } from 'mobx-react-lite';
import { Segment, Header, Comment, Loader } from 'semantic-ui-react';
import { useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Formik, Form, Field, FieldProps } from 'formik';
import * as Yup from 'yup';
import { formatDistanceToNow } from 'date-fns';

interface Props {
    activityId: string;
}

export default observer(function ActivityDetailedChat({ activityId }: Props) {
    const { commentStore } = useStore();

    useEffect(() => {
        //TODO: createHubConnection function will be to execute after the component is mounted, equivalent to componentDidMount
        if (activityId) {
            commentStore.createHubConnection(activityId);
        }
        //TODO: clearComments function will be to execute before the component unmounts and destroyed, equivalent to componentWillUnmount
        return () => {
            commentStore.clearComments();
        };
        //TODO: if we don't add activityId as a dependency, the useEffect will be executed only once, when the component is mounted, equivalent to componentDidMount
        //TODO: but if we add activityId as a dependency, the useEffect will be executed every time the activityId changes, equivalent to componentDidUpdate
    }, [commentStore, activityId]);

    // function handleKeyDown(e: KeyboardEvent<HTMLTextAreaElement>) {
    //     if (e.key === 'Enter' && e.shiftKey) {
    //         return;
    //     }
    //     if (e.key === 'Enter' && !e.shiftKey) {
    //         e.preventDefault();
    //         e.currentTarget.form?.requestSubmit();
    //     }
    // }

    return (
        <>
            <Segment textAlign="center" attached="top" inverted color="teal" style={{ border: 'none' }}>
                <Header>Chat about this event</Header>
            </Segment>
            <Segment attached clearing>
                <Formik
                    onSubmit={(value, { resetForm }) => commentStore.addComment(value).then(() => resetForm())}
                    initialValues={{ body: '' }}
                    validationSchema={Yup.object({
                        body: Yup.string().required(),
                    })}
                >
                    {({ isSubmitting, isValid, handleSubmit }) => (
                        <Form className="ui form">
                            <Field name="body">
                                {(props: FieldProps) => (
                                    <div style={{ position: 'relative' }}>
                                        <Loader active={isSubmitting} />
                                        <textarea
                                            placeholder="Enter your comment (Enter to submit, SHIFT + enter for new line)"
                                            rows={2}
                                            {...props.field}
                                            onKeyDown={(e) => {
                                                if (e.key === 'Enter' && e.shiftKey) {
                                                    return;
                                                }
                                                if (e.key === 'Enter' && !e.shiftKey) {
                                                    e.preventDefault();
                                                    isValid && handleSubmit();
                                                }
                                            }}
                                        />
                                    </div>
                                )}
                            </Field>
                        </Form>
                    )}
                </Formik>
                <Comment.Group>
                    {commentStore.comments.map((comment) => (
                        <Comment key={comment.id}>
                            <Comment.Avatar src={comment.image || '/assets/user.png'} />
                            <Comment.Content>
                                <Comment.Author as={Link} to={`/profiles/${comment.username}`}>
                                    {comment.displayName}
                                </Comment.Author>
                                <Comment.Metadata>
                                    <div>{formatDistanceToNow(comment.createdAt)}</div>
                                </Comment.Metadata>
                                {/* pre-wrap: Sequences of white space are preserved. Lines are broken at newline characters, at <br>, and as necessary to fill line boxes. */}
                                <Comment.Text style={{ whiteSpace: 'pre-wrap' }}>{comment.body}</Comment.Text>
                            </Comment.Content>
                        </Comment>
                    ))}
                </Comment.Group>
            </Segment>
        </>
    );
});
