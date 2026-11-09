let carga;
const mapas = new WeakMap();
function cargarGoogle(clave) {
    if (window.google?.maps?.Map) return Promise.resolve();
    if (!carga) carga = new Promise((resolve, reject) => {
        const script = document.createElement('script');
        let timeout;
        const fallo = () => {
            clearTimeout(timeout); script.remove(); carga = null;
            delete window.ssdapGoogleListo;
            reject(new Error('No se pudo cargar Google Maps'));
        };
        window.ssdapGoogleListo = () => { clearTimeout(timeout); delete window.ssdapGoogleListo; resolve(); };
        const parametros = new URLSearchParams({ key: clave, loading: 'async', callback: 'ssdapGoogleListo', v: 'quarterly', language: 'es', region: 'AR' });
        script.src = `https://maps.googleapis.com/maps/api/js?${parametros}`;
        script.async = true; script.onerror = fallo;
        timeout = setTimeout(fallo, 20000);
        document.head.append(script);
    });
    return carga;
}
export async function crear(elemento, receptor, latitud, longitud, clave) {
    if (!clave) throw new Error('Falta configurar Google Maps');
    await cargarGoogle(clave);
    if (!elemento.isConnected) return;
    destruir(elemento);
    const tienePunto = latitud != null && longitud != null;
    const centro = tienePunto ? { lat: latitud, lng: longitud } : { lat: -36.62, lng: -64.29 };
    const mapa = new google.maps.Map(elemento, { center: centro, zoom: tienePunto ? 15 : 7, streetViewControl: false, mapTypeControl: true, fullscreenControl: true });
    // Una cruz identifica el punto exacto sin necesitar un Map ID adicional.
    const cruz = document.createElement('span');
    cruz.textContent = '+';
    cruz.style.cssText = 'color:#a71919;font:bold 36px sans-serif;position:absolute;transform:translate(-50%,-50%);pointer-events:none;text-shadow:0 0 3px white';
    class Punto extends google.maps.OverlayView {
        onAdd() { this.getPanes().overlayMouseTarget.append(cruz); }
        draw() { if (!this.posicion) return; const p = this.getProjection().fromLatLngToDivPixel(this.posicion); cruz.style.left = `${p.x}px`; cruz.style.top = `${p.y}px`; }
        onRemove() { cruz.remove(); }
    }
    const punto = new Punto();
    if (tienePunto) { punto.posicion = new google.maps.LatLng(centro); punto.setMap(mapa); }
    const listener = mapa.addListener('click', async e => {
        if (!e.latLng) return;
        punto.posicion = e.latLng;
        if (!punto.getMap()) punto.setMap(mapa); else punto.draw();
        try { await receptor.invokeMethodAsync('Elegir', e.latLng.lat(), e.latLng.lng()); }
        catch { /* El circuito puede haberse cerrado. */ }
    });
    mapas.set(elemento, { mapa, punto, listener });
}
export function destruir(elemento) {
    const estado = mapas.get(elemento);
    if (estado) {
        estado.listener?.remove(); estado.punto?.setMap(null);
        estado.puntos?.forEach(p => p.setMap(null));
        estado.info?.close();
        google.maps.event.clearInstanceListeners(estado.mapa);
        elemento.replaceChildren(); mapas.delete(elemento);
    }
}

export async function mostrarPozos(elemento, pozos, clave) {
    if (!clave) throw new Error('Falta configurar Google Maps');
    await cargarGoogle(clave);
    if (!elemento.isConnected) return;
    destruir(elemento);
    const mapa = new google.maps.Map(elemento, { center: { lat: pozos[0].latitud, lng: pozos[0].longitud }, zoom: 15, streetViewControl: false });
    const info = new google.maps.InfoWindow();
    const limites = new google.maps.LatLngBounds();
    const grupos = new Map();
    for (const pozo of pozos) {
        const clavePunto = `${pozo.latitud},${pozo.longitud}`;
        if (!grupos.has(clavePunto)) grupos.set(clavePunto, []);
        grupos.get(clavePunto).push(pozo);
    }
    class MarcaPozo extends google.maps.OverlayView {
        constructor(grupo) {
            super();
            this.posicion = new google.maps.LatLng(grupo[0].latitud, grupo[0].longitud);
            this.boton = document.createElement('button');
            this.boton.type = 'button';
            this.boton.className = 'well-map-marker';
            const estados = new Set(grupo.map(p => p.estado));
            this.boton.dataset.estado = estados.size > 1 ? 'Mixto' : grupo[0].estado;
            this.boton.textContent = grupo.length > 1 ? `${grupo.length} pozos` : grupo[0].codigo;
            this.boton.title = grupo.map(p => `${p.codigo} · ${p.nombre} · ${p.estado}`).join('\n');
            this.boton.addEventListener('click', () => {
                const contenido = document.createElement('div');
                contenido.className = 'well-map-info';
                for (const pozo of grupo) {
                    const nombre = document.createElement('strong');
                    nombre.textContent = `${pozo.codigo} · ${pozo.nombre}`;
                    const datos = document.createElement('p');
                    datos.textContent = `${pozo.estado} · ${pozo.caudal.toLocaleString('es-AR')} m³/h`;
                    contenido.append(nombre, datos);
                }
                info.setContent(contenido); info.setPosition(this.posicion); info.open({ map: mapa });
            });
            google.maps.OverlayView.preventMapHitsAndGesturesFrom(this.boton);
        }
        onAdd() { this.getPanes().overlayMouseTarget.append(this.boton); }
        draw() {
            const pixel = this.getProjection().fromLatLngToDivPixel(this.posicion);
            if (!pixel) return;
            this.boton.style.left = `${pixel.x}px`; this.boton.style.top = `${pixel.y}px`;
        }
        onRemove() { this.boton.remove(); }
    }
    const puntos = [];
    for (const grupo of grupos.values()) {
        const punto = new MarcaPozo(grupo); punto.setMap(mapa); puntos.push(punto); limites.extend(punto.posicion);
    }
    if (grupos.size > 1) mapa.fitBounds(limites, 65);
    mapas.set(elemento, { mapa, puntos, info });
}
