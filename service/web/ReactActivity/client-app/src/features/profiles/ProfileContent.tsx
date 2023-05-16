import ProfilePhotos from './ProfilePhotos';
import { Profile } from '../../app/models/profile';
import { useStore } from '../../app/stores/store';
import ProfileFollowings from './ProfileFollowings';
import ProfileEdit from './ProfileEdit';
import ProfileActivities from './ProfileActivities';

import { Tab } from 'semantic-ui-react';
import { useEffect } from 'react';
import { observer } from 'mobx-react-lite';

interface Props {
    profile: Profile;
}

export default observer(function ProfileContent({ profile }: Props) {
    const { profileStore } = useStore();
    const { profileContentTabStyleLoaded, profileContentTabStyle, setProfileContentTabStyleComponentSize } =
        profileStore;

    const panes = [
        { menuItem: 'About', render: () => <ProfileEdit /> },
        { menuItem: 'Photos', render: () => <ProfilePhotos profile={profile} /> },
        { menuItem: 'Events', render: () => <ProfileActivities /> },
        { menuItem: 'Followers', render: () => <ProfileFollowings /> },
        { menuItem: 'Following', render: () => <ProfileFollowings /> },
    ];

    useEffect(() => {
        if (!profileContentTabStyleLoaded) {
            setProfileContentTabStyleComponentSize();
        }
    }, [profileContentTabStyleLoaded, setProfileContentTabStyleComponentSize]);

    return (
        <Tab
            menu={{ fluid: profileContentTabStyle.fluid, vertical: profileContentTabStyle.vertical }}
            menuPosition={profileContentTabStyle.menuPosition as any}
            panes={panes}
            onTabChange={(e, data) => profileStore.setActiveTab(data.activeIndex)}
        />
    );
});
