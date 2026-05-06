// Dropzone helper:
// - Captures the browser drop event to store FileList
// - Lets Blazor @ondrop handler forward those files into an <input type="file">,
//   which triggers Blazor's <InputFile OnChange> pipeline.
window.teacherAppAdminDropzone = (() => {
  let lastDropFiles = null;

  function register(dropzoneId) {
    const dz = document.getElementById(dropzoneId);
    if (!dz) return;
    if (dz.__teacherAppDropzoneRegistered) return;
    dz.__teacherAppDropzoneRegistered = true;

    dz.addEventListener(
      "drop",
      (e) => {
        try {
          lastDropFiles = e?.dataTransfer?.files ?? null;
        } catch {
          lastDropFiles = null;
        }
      },
      true // capture so it runs before Blazor's handler
    );
  }

  function forwardLastDropToInput(inputId) {
    if (!lastDropFiles || lastDropFiles.length === 0) return false;

    const input = document.getElementById(inputId);
    if (!input) return false;

    const dt = new DataTransfer();
    for (const f of lastDropFiles) dt.items.add(f);
    input.files = dt.files;

    input.dispatchEvent(new Event("change", { bubbles: true }));
    lastDropFiles = null;
    return true;
  }

  return { register, forwardLastDropToInput };
})();

