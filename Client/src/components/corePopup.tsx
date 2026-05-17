import { useState } from 'react';
import '../styles/Popup.css';
import CoreButton from './coreButton';

interface Props {
    message: string;
    onClose?: () => void;
}

export default function CorePopup({ message, onClose }: Props) {
    const [isVisible, setIsVisible] = useState(true);

    console.log("Rendering CorePopup with message:", message);

    return isVisible ? (
        <div className="popup-overlay" onClick={() => setIsVisible(false)}>
            <div className="popup-content" onClick={e => e.stopPropagation()}>
                <p>{message}</p>
                <CoreButton text="Close" onClick={() => {
                    setIsVisible(false);
                    onClose?.();
                }} />
            </div>
        </div>
    ) : <></>;

}