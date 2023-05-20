import agent from '../api/agent';
import { Activity, ActivityFormValues } from '../models/activity';
import { store } from './store';
import { Profile } from '../models/profile';
import { Pagination, PagingParams } from '../models/pagination';

import { makeAutoObservable, reaction, runInAction } from 'mobx';
import { format } from 'date-fns';

export default class ActivityStore {
    activityRegistry = new Map<string, Activity>();
    currSelectedActivity: Activity | undefined = undefined;
    editMode = false;
    loading = false;
    loadingInitial = false;
    pagination: Pagination | null = null;
    pagingParams = new PagingParams();
    predicate = new Map().set('all', true);
    activityDashboardSize = {
        activityListItemWidth: 10,
        activityFiltersSidebarWidth: 6,
    };
    dashBoardSizeLoaded = false;
    activityDetailsSize = {
        activityDetailedCardWidth: 10,
        activityDetailedSidebarWidth: 6,
    };
    detailsSizeLoaded = false;

    constructor() {
        makeAutoObservable(this);

        reaction(
            () => this.predicate.keys(),
            () => {
                this.pagingParams = new PagingParams();
                this.activityRegistry.clear();
                this.loadActivities();
            }
        );
    }

    setPagingParams = (pagingParams: PagingParams) => {
        this.pagingParams = pagingParams;
    };

    setPredicate = (predicate: string, value: string | Date) => {
        const resetPredicate = () => {
            this.predicate.forEach((value, key) => {
                if (key !== 'startDate') {
                    this.predicate.delete(key);
                }
            });
        };
        switch (predicate) {
            case 'all':
                resetPredicate();
                this.predicate.set('all', true);
                break;
            case 'isGoing':
                resetPredicate();
                this.predicate.set('isGoing', true);
                break;
            case 'isHost':
                resetPredicate();
                this.predicate.set('isHost', true);
                break;
            case 'startDate':
                resetPredicate();
                this.predicate.set('startDate', value);
                break;
        }
    };

    get axiosParams() {
        const params = new URLSearchParams();
        params.append('pageNumber', this.pagingParams.pageNumber.toString());
        params.append('pageSize', this.pagingParams.pageSize.toString());
        this.predicate.forEach((value, key) => {
            if (key === 'startDate') {
                params.append(key, (value as Date).toISOString());
            } else {
                params.append(key, value);
            }
        });
        return params;
    }

    get activitiesByDate() {
        // return Array.from(this.activityRegistry.values())
        //     .sort((a, b) => a.date!.getTime() - b.date!.getTime())
        //     .reverse();
        return Array.from(this.activityRegistry.values()).sort((a, b) => a.date!.getTime() - b.date!.getTime());
    }

    get groupActivities() {
        return Object.entries(
            this.activitiesByDate.reduce((activities, activity) => {
                const date = format(activity.date!, 'dd MMM yyyy');
                //TODO: If the date does exists, then update the activities by date, otherwise create a new one
                activities[date] = activities[date] ? [...activities[date], activity] : [activity];
                return activities;
            }, {} as { [key: string]: Activity[] })
        );
    }

    loadActivities = async () => {
        this.setLoadingInitial(true);
        try {
            const result = await agent.Activities.list(this.axiosParams);
            result.data.forEach((activity) => {
                this.setActivity(activity);
            });
            this.setPagination(result.pagination);
            this.setLoadingInitial(false);
        } catch (error) {
            console.log(error);
            this.setLoadingInitial(false);
        }
    };

    setPagination = (pagination: Pagination) => {
        this.pagination = pagination;
    };

    loadActivity = async (id: string) => {
        let activity = this.getActivity(id);
        if (activity) {
            this.currSelectedActivity = activity;
            return activity;
        } else {
            this.setLoadingInitial(true);
            try {
                activity = await agent.Activities.details(id);
                this.setActivity(activity);
                runInAction(() => {
                    this.currSelectedActivity = activity;
                });
                this.setLoadingInitial(false);
                return activity;
            } catch (error) {
                console.log(error);
                this.setLoadingInitial(false);
            }
        }
    };

    private setActivity = async (activity: Activity) => {
        const user = store.userStore.user;
        if (user) {
            activity.isGoing = activity.attendees!.some((a) => a.username === user.username);
            activity.isHost = activity.hostUsername === user.username;
            activity.host = activity.attendees?.find((x) => x.username === activity.hostUsername);
        }
        activity.date = new Date(activity.date!);
        this.activityRegistry.set(activity.id, activity);
    };

    private getActivity = (id: string) => {
        return this.activityRegistry.get(id);
    };

    setLoadingInitial = (state: boolean) => {
        this.loadingInitial = state;
    };

    createActivity = async (activity: ActivityFormValues) => {
        const user = store.userStore.user;
        const attendee = new Profile(user!);
        try {
            await agent.Activities.create(activity);
            const newActivity = new Activity(activity);
            newActivity.hostUsername = user!.username;
            newActivity.attendees = [attendee];
            this.setActivity(newActivity);
            runInAction(() => {
                this.currSelectedActivity = newActivity;
            });
        } catch (error) {
            console.log(error);
        }
    };

    updateActivity = async (activity: ActivityFormValues) => {
        try {
            await agent.Activities.update(activity);
            runInAction(() => {
                if (activity.id) {
                    let updatedActivity = { ...this.getActivity(activity.id), ...activity };
                    this.activityRegistry.set(activity.id, updatedActivity as Activity);
                    this.currSelectedActivity = updatedActivity as Activity;
                }
            });
        } catch (error) {
            console.log(error);
        }
    };

    deleteActivity = async (id: string) => {
        this.loading = true;
        try {
            await agent.Activities.delete(id);
            runInAction(() => {
                this.activityRegistry.delete(id);
                this.loading = false;
            });
        } catch (error) {
            console.log(error);
            runInAction(() => {
                this.loading = false;
            });
        }
    };

    updateAttendance = async () => {
        const user = store.userStore.user;
        this.loading = true;
        try {
            await agent.Activities.attend(this.currSelectedActivity!.id);
            runInAction(() => {
                if (this.currSelectedActivity?.isGoing) {
                    this.currSelectedActivity.attendees = this.currSelectedActivity.attendees?.filter(
                        (a) => a.username !== user?.username
                    );
                    this.currSelectedActivity.isGoing = false;
                } else {
                    const attendee = new Profile(user!);
                    this.currSelectedActivity?.attendees?.push(attendee);
                    this.currSelectedActivity!.isGoing = true;
                }
                // console.log('this.currSelectedActivity!.isGoing: ', this.currSelectedActivity!.isGoing);
                this.activityRegistry.set(this.currSelectedActivity!.id, this.currSelectedActivity!);
            });
        } catch (error) {
            console.log(error);
        } finally {
            runInAction(() => {
                this.loading = false;
            });
        }
    };

    cancelActivityToggle = async () => {
        this.loading = true;
        try {
            await agent.Activities.attend(this.currSelectedActivity!.id);
            runInAction(() => {
                this.currSelectedActivity!.isCancelled = !this.currSelectedActivity?.isCancelled;
                this.activityRegistry.set(this.currSelectedActivity!.id, this.currSelectedActivity!);
            });
        } catch (error) {
            console.log(error);
        } finally {
            runInAction(() => {
                this.loading = false;
            });
        }
    };

    clearSelectedActivity = () => {
        this.currSelectedActivity = undefined;
    };

    updateAttendeeFollowing = (username: string) => {
        this.activityRegistry.forEach((activity) => {
            activity.attendees?.forEach((attendee) => {
                if (attendee.username === username) {
                    attendee.following ? attendee.followersCount-- : attendee.followersCount++;
                    attendee.following = !attendee.following;
                }
            });
        });
    };

    setActivityDashboardComponentSize = () => {
        const detectedMobileDevice = store.commonStore.detectedMobileDevice;
        if (!detectedMobileDevice) {
            this.activityDashboardSize.activityListItemWidth = 10;
            this.activityDashboardSize.activityFiltersSidebarWidth = 6;
        } else {
            this.activityDashboardSize.activityListItemWidth = 14;
            this.activityDashboardSize.activityFiltersSidebarWidth = 12;
        }
        this.dashBoardSizeLoaded = true;
    };

    setActivityDetailsComponentSize = () => {
        const detectedMobileDevice = store.commonStore.detectedMobileDevice;
        if (!detectedMobileDevice) {
            this.activityDetailsSize.activityDetailedCardWidth = 10;
            this.activityDetailsSize.activityDetailedSidebarWidth = 6;
        } else {
            this.activityDetailsSize.activityDetailedCardWidth = 15;
            this.activityDetailsSize.activityDetailedSidebarWidth = 12;
        }
        this.detailsSizeLoaded = true;
    };
}
