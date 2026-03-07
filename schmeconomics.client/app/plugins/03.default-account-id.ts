export default defineNuxtPlugin(async () => {
    const defaultAccountId = ref(
        process.client ? 
            localStorage.getItem("defaultAccountId") || "" :
            ""
    );

    watch(
        defaultAccountId, 
        () => {
            if(process.client) {
                localStorage.setItem("defaultAccountId", defaultAccountId.value);
            }
        }
    );

    return { 
        provide: {
            defaultAccountId
        }
    }
});