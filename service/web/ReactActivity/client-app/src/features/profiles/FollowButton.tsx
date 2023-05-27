import { Profile } from '../../app/models/profile';
import { useStore } from '../../app/stores/store';

import { observer } from 'mobx-react-lite';
import { Button, Reveal } from 'semantic-ui-react';
import { SyntheticEvent } from 'react';
import { useLocation } from 'react-router-dom';

interface Props {
    profile: Profile;
}

export default observer(function FollowButton({ profile }: Props) {
    const { profileStore, userStore } = useStore();
    const { updateFollowing, loading } = profileStore;
    const location = useLocation();
    const [loadProfileUserName] = location.pathname.split('/').slice(-1);
    const [loadPageFeatureName] = location.pathname.split('/').slice(1);

    if (
        userStore.user?.username === profile.username ||
        (loadPageFeatureName === 'profiles' && loadProfileUserName !== userStore.user?.username)
    ) {
        return null;
    }

    function handleFollow(e: SyntheticEvent, username: string) {
        e.preventDefault();
        profile.following ? updateFollowing(username, false) : updateFollowing(username, true);
    }

    return (
        <Reveal animated="move">
            <Reveal.Content visible style={{ width: '100%' }}>
                <Button fluid color="teal" content={profile.following ? 'Following' : 'Not following'} />
            </Reveal.Content>
            <Reveal.Content hidden style={{ width: '100%' }}>
                <Button
                    fluid
                    color={profile.following ? 'red' : 'green'}
                    content={profile.following ? 'Unfollow' : 'Follow'}
                    loading={loading}
                    onClick={(e) => handleFollow(e, profile.username)}
                />
            </Reveal.Content>
        </Reveal>
    );
});
