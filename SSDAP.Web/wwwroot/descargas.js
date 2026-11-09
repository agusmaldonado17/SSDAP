export async function descargarPdf(nombre, referencia) {
    const buffer = await referencia.arrayBuffer();
    const url = URL.createObjectURL(new Blob([buffer], { type: "application/pdf" }));
    const enlace = document.createElement("a");
    enlace.href = url;
    enlace.download = nombre;
    document.body.appendChild(enlace);
    enlace.click();
    enlace.remove();
    setTimeout(() => URL.revokeObjectURL(url), 30000);
}
