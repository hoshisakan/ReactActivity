import { Photo, Profile } from '../../app/models/profile';
import { useStore } from '../../app/stores/store';
import PhotoUploadWidget from '../../app/common/imageUpload/PhotoUploadWidget';

import { observer } from 'mobx-react-lite';
import { Card, Header, Tab, Image, Grid, Button, SemanticWIDTHS } from 'semantic-ui-react';
import { SyntheticEvent, useEffect, useState } from 'react';

interface Props {
    profile: Profile;
}

export default observer(function ProfilePhotos({ profile }: Props) {
    const {
        profileStore: {
            isCurrentUser,
            uploadPhoto,
            uploading,
            loading,
            setMainPhoto,
            deletePhoto,
            profileContentPhotosSizeLoaded,
            profileContentPhotosSize,
            setProfileContentPhotosComponentSize,
            profileContentPhotoUploadWidgetsSizeLoaded,
            setProfileContentPhotoUploadWidgetsComponentSize,
        },
    } = useStore();
    const [addPhotoMode, setAddPhotoMode] = useState(false);
    const [target, setTarget] = useState('');

    function handlePhotoUpload(file: Blob) {
        uploadPhoto(file).then(() => setAddPhotoMode(false));
    }

    function handleSetMainPhoto(photo: Photo, e: SyntheticEvent<HTMLButtonElement>) {
        setMainPhoto(photo);
        setTarget(e.currentTarget.name);
    }

    function handleDeletePhoto(photo: Photo, e: SyntheticEvent<HTMLButtonElement>) {
        setTarget(e.currentTarget.name);
        deletePhoto(photo);
    }

    useEffect(() => {
        if (!profileContentPhotosSizeLoaded) {
            setProfileContentPhotosComponentSize();
        }
        if (!profileContentPhotoUploadWidgetsSizeLoaded) {
            setProfileContentPhotoUploadWidgetsComponentSize();
        }
    }, [profileContentPhotoUploadWidgetsSizeLoaded, profileContentPhotosSizeLoaded, setProfileContentPhotoUploadWidgetsComponentSize, setProfileContentPhotosComponentSize]);

    return (
        <Tab.Pane>
            <Grid>
                <Grid.Column width={16}>
                    <Header floated="left" icon="image" content="Photos" />
                    {isCurrentUser && (
                        <Button
                            floated="right"
                            basic
                            content={addPhotoMode ? 'Cancel' : 'Add Photo'}
                            onClick={() => setAddPhotoMode(!addPhotoMode)}
                        />
                    )}
                </Grid.Column>
                <Grid.Column width={16}>
                    {addPhotoMode ? (
                        <PhotoUploadWidget uploadPhoto={handlePhotoUpload} loading={uploading} />
                    ) : (
                        <Card.Group itemsPerRow={profileContentPhotosSize.cardGroupItemsPerRow as SemanticWIDTHS}>
                            {profile.photos?.map((photo) => (
                                <Card key={photo.id}>
                                    <Image src={photo.url} />
                                    {isCurrentUser && (
                                        <Button.Group fluid widths={2}>
                                            <Button
                                                basic
                                                color="green"
                                                content="Main"
                                                name={'main' + photo.id}
                                                disabled={photo.isMain}
                                                loading={target === 'main' + photo.id && loading}
                                                onClick={(e) => handleSetMainPhoto(photo, e)}
                                            />
                                            <Button
                                                icon="trash"
                                                basic
                                                color="red"
                                                name={photo.id}
                                                disabled={photo.isMain}
                                                loading={target === photo.id && loading}
                                                onClick={(e) => handleDeletePhoto(photo, e)}
                                            ></Button>
                                        </Button.Group>
                                    )}
                                </Card>
                            ))}
                        </Card.Group>
                    )}
                </Grid.Column>
            </Grid>
        </Tab.Pane>
    );
});
