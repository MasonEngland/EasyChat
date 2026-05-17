import '../styles/coreButton.css';

interface Props {
    text: string;
    onClick: () => void;
    disabled?: boolean;
}

export default function CoreButton({ text, onClick, disabled = false }: Props) {
    return (<button className="core-button" onClick={onClick} disabled={disabled}>
        {text}
    </button>);

}
