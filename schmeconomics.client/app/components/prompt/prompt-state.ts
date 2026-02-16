export interface PromptState {
    visible: boolean,
    data: PromptData
}

export interface PromptData {
    message: string,
    actions: [string, () => Promise<void>][]
}

export function showPrompt(data: PromptData) {
    const state = usePromptState();

    state.value.visible = true;
    state.value.data = data;
}

export const usePromptState = () => useState<PromptState>(
    'promptState',
    () => {
        return {
            visible: false,
            data: { message: '', actions: [] }
        }
    }
)