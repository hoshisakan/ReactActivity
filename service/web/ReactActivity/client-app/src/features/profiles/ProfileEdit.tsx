import { useStore } from '../../app/stores/store';
import ProfileAbout from './ProfileEditForm';

import {  useState } from 'react';
import { Button, Grid, Header, Tab } from 'semantic-ui-react';

export default function ProfileEdit() {
    const {
        profileStore, commonStore
    } = useStore();
    const { profile, isCurrentUser } = profileStore;
    const { detectedMobileDevice } = commonStore;
    const [editMode, setEditMode] = useState(false);

    return (
        <Tab.Pane>
            <Grid>
                <Grid.Column width={16}>
                    <Header floated="left" icon="user" content={`About ${profile?.displayName}`} />
                    {isCurrentUser && (
                        <Button
                            floated={detectedMobileDevice ? 'left' : 'right'}
                            basic
                            content={editMode ? 'Cancel' : 'Edit Profile'}
                            onClick={() => setEditMode(!editMode)}
                        />
                    )}
                </Grid.Column>
                <Grid.Column width={16}>
                    {editMode ? (
                        <ProfileAbout setEditMode={setEditMode} />
                    ) : (
                        <span style={{ whiteSpace: 'pre-wrap' }}>{profile?.bio}</span>
                    )}
                </Grid.Column>
            </Grid>
        </Tab.Pane>
    );
}
