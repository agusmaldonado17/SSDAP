let timer, callback, pending = false, busy = false;
const events = ['pointerdown', 'keydown', 'input', 'wheel', 'touchstart'];
function activity(event) {
    if (event.isTrusted) pending = true;
}
async function visibility() {
    if (document.hidden) { pending = false; return; }
    if (busy || !callback) return;
    busy = true;
    try { await callback.invokeMethodAsync('ReanudarAsync'); }
    catch { /* El componente puede haberse cerrado durante la comprobación. */ }
    finally { busy = false; }
}
export function iniciar(reference) {
    detener();
    callback = reference;
    document.addEventListener('visibilitychange', visibility);
    events.forEach(name => document.addEventListener(name, activity, { passive: true }));
    timer = setInterval(async () => {
        if (busy || document.hidden) return;
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
    document.removeEventListener('visibilitychange', visibility);
    events.forEach(name => document.removeEventListener(name, activity));
    pending = false;
    callback = null;
}
