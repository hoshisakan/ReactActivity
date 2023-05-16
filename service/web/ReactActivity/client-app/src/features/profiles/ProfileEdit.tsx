import { useStore } from '../../app/stores/store';
import ProfileAbout from './ProfileEditForm';

import { useEffect, useState } from 'react';
import { Button, Grid, Header, Tab } from 'semantic-ui-react';
import { observer } from 'mobx-react-lite';

//TODO: Add observer for auto refresh when data change
export default observer(function ProfileEdit() {
    const { profileStore } = useStore();
    const {
        profile,
        isCurrentUser,
        profileEditPageStyleLoaded,
        profileEditPageStyle,
        setProfileEditPageStyleComponentSize,
    } = profileStore;
    const [editMode, setEditMode] = useState(false);

    useEffect(() => {
        if (!profileEditPageStyleLoaded) {
            setProfileEditPageStyleComponentSize();
        }
    }, [profileEditPageStyleLoaded, setProfileEditPageStyleComponentSize]);

    return (
        <Tab.Pane>
            <Grid>
                <Grid.Column width={16}>
                    <Header floated="left" icon="user" content={`About ${profile?.displayName}`} />
                    {isCurrentUser && (
                        <Button
                            floated={profileEditPageStyle.buttonFloated as 'left' | 'right'}
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
});
