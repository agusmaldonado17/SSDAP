let timer, callback, pending = false, busy = false;
const events = ['pointerdown', 'keydown', 'input', 'wheel', 'touchstart'];
function activity(event) {
    if (event.isTrusted) pending = true;
}
export function iniciar(reference) {
    detener();
    callback = reference;
    events.forEach(name => document.addEventListener(name, activity, { passive: true }));
    timer = setInterval(async () => {
        if (busy) return;
        busy = true;
        try {
            if (pending) {
                pending = false;
                await callback.invokeMethodAsync('InformarActividad');
            }
            await callback.invokeMethodAsync('ComprobarVencimiento');
        } catch { /* Blazor muestra el estado de reconexión. */ }
        finally { busy = false; }
    }, 15000);
}
export function detener() {
    clearInterval(timer);
    events.forEach(name => document.removeEventListener(name, activity));
    pending = false;
}
