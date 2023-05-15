import PhotoWidgetDropzone from './PhotoWidgetDropzone';

import { useEffect, useState } from 'react';
import { Button, Grid, Header, SemanticWIDTHS } from 'semantic-ui-react';
import PhotoWidgetCropper from './PhotoWidgetCropper';
import { useStore } from '../../stores/store';

interface Props {
    uploadPhoto: (file: Blob) => void;
    loading: boolean;
}

export default function PhotoUploadWidget({ loading, uploadPhoto }: Props) {
    const [files, setFiles] = useState<any>();
    const [cropper, setCropper] = useState<Cropper>();
    const {
        profileStore: { profileContentPhotoUploadWidgetsSize },
    } = useStore();

    function onCrop() {
        if (cropper) {
            cropper.getCroppedCanvas().toBlob((blob) => uploadPhoto(blob!));
        }
    }

    useEffect(() => {
        return () => {
            files?.forEach((file: any) => URL.revokeObjectURL(file.preview));
        };
    }, [files]);

    return (
        <Grid>
            <Grid.Column width={profileContentPhotoUploadWidgetsSize.dropzoneCardGroupColumnWidth as SemanticWIDTHS}>
                <Header color="teal" sub content="Step 1 - Add Photo" />
                <PhotoWidgetDropzone setFiles={setFiles} />
            </Grid.Column>
            <Grid.Column width={1} />
            <Grid.Column width={profileContentPhotoUploadWidgetsSize.cropPhotoCardGroupColumnWidth as SemanticWIDTHS}>
                <Header color="teal" sub content="Step 2 - Resize image" />
                {files && files.length > 0 && (
                    <PhotoWidgetCropper setCropper={setCropper} imagePreview={files[0].preview} />
                )}
            </Grid.Column>
            <Grid.Column width={1} />
            <Grid.Column
                width={profileContentPhotoUploadWidgetsSize.previewPhotoCardGroupColumnWidth as SemanticWIDTHS}
            >
                <Header color="teal" sub content="Step 3 - Preview & Upload" />
                {files && files.length > 0 && (
                    <>
                        <div className="img-preview" style={{ minHeight: 200, overflow: 'hidden' }} />
                        <Button.Group widths={2}>
                            <Button loading={loading} onClick={onCrop} positive icon="check" />
                            <Button disabled={loading} onClick={() => setFiles([])} icon="close" />
                        </Button.Group>
                    </>
                )}
            </Grid.Column>
        </Grid>
    );
}
