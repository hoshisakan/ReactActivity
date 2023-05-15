import { useEffect } from 'react';
import { useStore } from '../../app/stores/store';
import ProfileCard from './ProfileCard';

import { observer } from 'mobx-react-lite';
import { Card, Grid, Header, SemanticWIDTHS, Tab } from 'semantic-ui-react';

export default observer(function ProfileFollowings() {
    const { profileStore } = useStore();
    const {
        profile,
        followings,
        loadingFollowings,
        activeTab,
        profileContentFollowingsSize,
        profileContentFollowingsSizeLoaded,
        setProfileContentFollowingsComponentSize,
    } = profileStore;

    useEffect(() => {
        if (!profileContentFollowingsSizeLoaded) {
            setProfileContentFollowingsComponentSize();
        }
    }, [profileContentFollowingsSizeLoaded, setProfileContentFollowingsComponentSize]);

    return (
        <Tab.Pane loading={loadingFollowings}>
            <Grid>
                <Grid.Column width={16}>
                    <Header
                        floated="left"
                        icon="user"
                        content={
                            activeTab === 3
                                ? `People following ${profile?.displayName}`
                                : `People ${profile?.displayName} is following`
                        }
                    />
                </Grid.Column>
                <Grid.Column width={16}>
                    <Card.Group itemsPerRow={profileContentFollowingsSize.cardGroupItemsPerRow as SemanticWIDTHS}>
                        {followings.map((profile) => (
                            <ProfileCard key={profile.username} profile={profile} />
                        ))}
                    </Card.Group>
                </Grid.Column>
            </Grid>
        </Tab.Pane>
    );
});
