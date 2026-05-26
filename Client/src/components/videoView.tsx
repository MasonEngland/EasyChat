import { useEffect, useRef, useState } from "react";
import "../styles/videoView.css";
import CoreButton from "./coreButton";
import connection from "../lib/signalr";
import config from "../lib/config";

interface Props {
    roomId: string;
    host: boolean;
    setIsStreaming: (value: boolean) => void;
    setIsHost: (value: boolean) => void;
}

export default function VideoView({roomId, host, setIsStreaming, setIsHost} : Props) {

    const videoRef = useRef<HTMLVideoElement>(null);

    const broadCastSreamUpdate = (isStopped: boolean = false) => {
        if (!host) return;
        const videoElement = videoRef.current;
        if (!videoElement) return;

        const timestamp = videoElement.currentTime;
        const isPaused = videoElement.paused;
        const isMuted = videoElement.muted;

        connection.invoke("BroadcastStreamUpdate", roomId, timestamp, isPaused, isMuted, isStopped);
        
    }

    useEffect(() => {
        const videoElement = videoRef.current;
        if (!videoElement) return;
        videoElement.play().catch(err => {
            console.error("Error playing video: ", err);
            setIsStreaming(false);
        });
        if (!host) {
            connection.on("ReceiveStreamUpdate", (timestamp: number, isPaused: boolean, isMuted: boolean, isStopped: boolean) => {
                const videoElement = videoRef.current;

                if (isStopped) {
                    setIsStreaming(false);
                    setIsHost(false);
                    return;
                }
                if (!videoElement) return;

                if (isPaused) {
                    videoElement.pause();
                } else {
                    videoElement.play();
                }

                if (Math.abs(videoElement.currentTime - timestamp) > 1.5) {
                    videoElement.currentTime = timestamp;
                }
                videoElement.muted = isMuted;
            });

            return () => {
                connection.off("ReceiveStreamUpdate");
            }
        }
    }, []);


    const handleStopStreaming = () => {
        setIsHost(false);
        broadCastSreamUpdate(true);
        setIsStreaming(false);
    }

    if (host) {
        return (
            <div className="video-overlay">
                <CoreButton text="Stop Streaming" onClick={() => {
                    handleStopStreaming();
                }} />
                <video 
                    controls 
                    onError={() => handleStopStreaming()}
                    onTimeUpdate={() => broadCastSreamUpdate()}
                    onPause={() => broadCastSreamUpdate()}
                    onPlay={() => broadCastSreamUpdate()}
                    ref={videoRef}
                 >
                    <source 
                        src={`${config.serverUrl}/Api/Streaming/Watch/${roomId}`} 
                        type="video/mp4"
                    />
                </video>
            </div>
        )
    }
    return (
        <div className="video-overlay">
            <video ref={videoRef} onError={() => handleStopStreaming()} >
                <source 
                    src={`${config.serverUrl}/Api/Streaming/Watch/${roomId}`} 
                    type="video/mp4" 
                />
            </video>
        </div>
    )

}