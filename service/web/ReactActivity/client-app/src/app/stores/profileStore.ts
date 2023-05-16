import agent from '../api/agent';
import { Photo, Profile, UserActivity } from '../models/profile';
import { store } from './store';

import { makeAutoObservable, reaction, runInAction } from 'mobx';

export default class ProfileStore {
    profile: Profile | null = null;
    loadingProfile = false;
    uploading = false;
    loading = false;
    followings: Profile[] = [];
    loadingFollowings = false;
    activeTab = 0;
    userActivities: UserActivity[] = [];
    loadingActivities = false;
    profileHeaderSize = {
        contentWidth: 12,
        featuresWidth: 4,
    };
    profileHeaderSizeLoaded = false;
    profileEditPageStyle = {
        buttonFloated: 'right',
    };
    profileEditPageStyleLoaded = false;
    profileContentFollowingsSize = {
        cardGroupItemsPerRow: 4,
    };
    profileContentFollowingsSizeLoaded = false;
    profileContentEventsSize = {
        cardGroupItemsPerRow: 4,
    };
    profileContentEventsSizeLoaded = false;
    profileContentPhotosSize = {
        cardGroupItemsPerRow: 5,
    };
    profileContentPhotosSizeLoaded = false;
    profileContentPhotoUploadWidgetsSize = {
        dropzoneCardGroupColumnWidth: 4,
        cropPhotoCardGroupColumnWidth: 4,
        previewPhotoCardGroupColumnWidth: 4,
    };
    profileContentPhotoUploadWidgetsSizeLoaded = false;
    profileContentTabStyle = {
        fluid: true,
        vertical: true,
        menuPosition: 'right',
    };

    profileContentTabStyleLoaded = false;

    constructor() {
        makeAutoObservable(this);

        reaction(
            () => this.activeTab,
            (activeTab) => {
                if (activeTab === 3 || activeTab === 4) {
                    const predicate = this.activeTab === 3 ? 'followers' : 'following';
                    this.loadFollowings(predicate);
                } else {
                    this.followings = [];
                }
            }
        );
    }

    setActiveTab = (activeTab: any) => {
        this.activeTab = activeTab;
    };

    get isCurrentUser() {
        if (store.userStore.user && this.profile) {
            return store.userStore.user.username === this.profile.username;
        }
        return false;
    }

    loadProfile = async (username?: string) => {
        this.loadingProfile = true;
        try {
            if (username) {
                const profile = await agent.Profiles.get(username);
                runInAction(() => {
                    this.profile = profile;
                    this.loadingProfile = false;
                });
            }
        } catch (error) {
            console.log(error);
            runInAction(() => (this.loadingProfile = false));
        }
    };

    updateProfile = async (profile: Partial<Profile>) => {
        // this.loading = true;
        try {
            await agent.Profiles.updateProfile(profile);
            runInAction(() => {
                if (profile.displayName && profile.displayName !== store.userStore.user?.displayName) {
                    store.userStore.setDisplayName(profile.displayName);
                }
                this.profile = { ...this.profile, ...(profile as Profile) };
                // this.loading = false;
            });
        } catch (error) {
            console.log(error);
            // runInAction(() => (this.loading = false));
        }
    };

    uploadPhoto = async (file: Blob) => {
        this.uploading = true;
        try {
            const response = await agent.Profiles.uploadPhoto(file);
            const photo = response.data;
            runInAction(() => {
                if (this.profile) {
                    this.profile.photos?.push(photo);
                    if (photo.isMain && store.userStore.user) {
                        store.userStore.setImage(photo.url);
                        this.profile.image = photo.url;
                    }
                }
                this.uploading = false;
            });
        } catch (error) {
            console.log(error);
            runInAction(() => (this.uploading = false));
        }
    };

    setMainPhoto = async (photo: Photo) => {
        this.loading = true;
        try {
            await agent.Profiles.setMainPhoto(photo.id);
            store.userStore.setImage(photo.url);
            runInAction(() => {
                if (this.profile && this.profile.photos) {
                    this.profile.photos.find((a) => a.isMain)!.isMain = false;
                    this.profile.photos.find((a) => a.id === photo.id)!.isMain = true;
                    this.profile.image = photo.url;
                    this.loading = false;
                }
            });
        } catch (error) {
            console.log(error);
            runInAction(() => (this.loading = false));
        }
    };

    deletePhoto = async (photo: Photo) => {
        this.loading = true;
        try {
            await agent.Profiles.deletePhoto(photo.id);
            runInAction(() => {
                if (this.profile) {
                    this.profile.photos = this.profile.photos?.filter((a) => a.id !== photo.id);
                    this.loading = false;
                }
            });
        } catch (error) {
            console.log(error);
            runInAction(() => (this.loading = false));
        }
    };

    updateFollowing = async (username: string, following: boolean) => {
        this.loading = true;
        try {
            await agent.Profiles.updateFollower(username);
            store.activityStore.updateAttendeeFollowing(username);
            runInAction(() => {
                if (
                    this.profile &&
                    this.profile.username !== store.userStore.user?.username &&
                    this.profile.username !== username
                ) {
                    following ? this.profile.followersCount++ : this.profile.followersCount--;
                    this.profile.following = !this.profile.following;
                } else if (this.profile && this.profile.username !== store.userStore.user?.username) {
                    following ? this.profile.followersCount++ : this.profile.followersCount--;
                    this.profile.following = !this.profile.following;
                } else if (this.profile && this.profile.username === store.userStore.user?.username) {
                    following ? this.profile.followingCount++ : this.profile.followingCount--;
                }
                this.followings.forEach((profile) => {
                    if (profile.username === username) {
                        profile.following ? profile.followersCount-- : profile.followersCount++;
                        profile.following = !profile.following;
                    }
                });
                this.loading = false;
            });
        } catch (error) {
            console.log(error);
            runInAction(() => (this.loading = false));
        }
    };

    loadFollowings = async (predicate: string) => {
        this.loadingFollowings = true;
        try {
            const currentUserFollowings = await agent.Profiles.listFollowings(this.profile!.username, predicate);
            runInAction(() => {
                this.followings = currentUserFollowings;
                this.loadingFollowings = false;
            });
        } catch (error) {
            console.log(error);
            runInAction(() => (this.loadingFollowings = false));
        }
    };

    loadUserActivities = async (username: string, predicate?: string) => {
        this.loadingActivities = true;
        try {
            const activities = await agent.Profiles.listActivities(username, predicate!);
            runInAction(() => {
                this.userActivities = activities;
                this.loadingActivities = false;
            });
        } catch (error) {
            console.log(error);
            runInAction(() => (this.loadingActivities = false));
        }
    };

    setProfileHeaderComponentSize = () => {
        const detectedMobileDevice = store.commonStore.detectedMobileDevice;
        if (!detectedMobileDevice) {
            this.profileHeaderSize.contentWidth = 12;
            this.profileHeaderSize.featuresWidth = 4;
        } else {
            this.profileHeaderSize.contentWidth = 7;
            this.profileHeaderSize.featuresWidth = 9;
        }
        this.profileHeaderSizeLoaded = true;
    };

    setProfileEditPageStyleComponentSize = () => {
        const detectedMobileDevice = store.commonStore.detectedMobileDevice;
        if (!detectedMobileDevice) {
            this.profileEditPageStyle.buttonFloated = 'right';
        } else {
            this.profileEditPageStyle.buttonFloated = 'left';
        }
        this.profileEditPageStyleLoaded = true;
    };

    setProfileContentFollowingsComponentSize = () => {
        const detectedMobileDevice = store.commonStore.detectedMobileDevice;
        if (!detectedMobileDevice) {
            this.profileContentFollowingsSize.cardGroupItemsPerRow = 4;
        } else {
            this.profileContentFollowingsSize.cardGroupItemsPerRow = 1;
        }
        this.profileContentFollowingsSizeLoaded = true;
    };

    setProfileContentEventsComponentSize = () => {
        const detectedMobileDevice = store.commonStore.detectedMobileDevice;
        if (!detectedMobileDevice) {
            this.profileContentEventsSize.cardGroupItemsPerRow = 4;
        } else {
            this.profileContentEventsSize.cardGroupItemsPerRow = 1;
        }
        this.profileContentEventsSizeLoaded = true;
    };

    setProfileContentPhotosComponentSize = () => {
        const detectedMobileDevice = store.commonStore.detectedMobileDevice;
        if (!detectedMobileDevice) {
            this.profileContentPhotosSize.cardGroupItemsPerRow = 5;
        } else {
            this.profileContentPhotosSize.cardGroupItemsPerRow = 1;
        }
        this.profileContentPhotosSizeLoaded = true;
    };

    setProfileContentPhotoUploadWidgetsComponentSize = () => {
        const detectedMobileDevice = store.commonStore.detectedMobileDevice;
        if (!detectedMobileDevice) {
            this.profileContentPhotoUploadWidgetsSize.dropzoneCardGroupColumnWidth = 4;
            this.profileContentPhotoUploadWidgetsSize.cropPhotoCardGroupColumnWidth = 4;
            this.profileContentPhotoUploadWidgetsSize.previewPhotoCardGroupColumnWidth = 4;
        } else {
            this.profileContentPhotoUploadWidgetsSize.dropzoneCardGroupColumnWidth = 14;
            this.profileContentPhotoUploadWidgetsSize.cropPhotoCardGroupColumnWidth = 14;
            this.profileContentPhotoUploadWidgetsSize.previewPhotoCardGroupColumnWidth = 14;
        }
        this.profileContentPhotoUploadWidgetsSizeLoaded = true;
    };

    setProfileContentTabStyleComponentSize = () => {
        const detectedMobileDevice = store.commonStore.detectedMobileDevice;
        if (!detectedMobileDevice) {
            this.profileContentTabStyle.fluid = true;
            this.profileContentTabStyle.vertical = true;
            this.profileContentTabStyle.menuPosition = 'right';
        } else {
            this.profileContentTabStyle.fluid = false;
            this.profileContentTabStyle.vertical = true;
            this.profileContentTabStyle.menuPosition = 'left';
        }
        this.profileContentTabStyleLoaded = true;
    };
}
