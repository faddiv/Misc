import { useCallback, useRef, useState } from "react";
import { Upload } from "tus-js-client";
import "./App.css";

function App() {
  const [data, setData] = useState<any>(null);
  const [percent, setPercent] = useState<number>(0);
  const fileInput = useRef<HTMLInputElement>(null);
  const tusUpload = useCallback(() => {
    if (!fileInput.current || !fileInput.current.files || fileInput.current.files.length === 0) return;
    var file = fileInput.current.files[0];
    console.log("Start upload");
    setPercent(0);
    const upload = new Upload(file, {
      endpoint: "api/Upload",
      chunkSize: 5*1024*1024,
      metadata: {
        filename: file.name,
        filetype: file.type,
      },
      onProgress: (bytesUploaded, bytesTotal) => {
        const p = Math.round((100.0 * bytesUploaded) / bytesTotal);
        console.log("Percent changed: ", p);
        setPercent(p);
      },
      onSuccess: () => {
        console.log("Upload finished");
        setPercent(100);
        setData("File upload successfull");
      },
      onError: (error) => {
        console.error(error);
        setData("File upload failed.");
      },
    });
    upload.start();
  }, []);
  return (
    <div className="App">
      <header className="App-header">
        <a className="App-link" href="https://reactjs.org" target="_blank" rel="noopener noreferrer">
          Learn React
        </a>
        <button
          onClick={async () => {
            setData(null);
            var response = await fetch("api/WeatherForecast");
            var result = await response.json();
            setData(JSON.stringify(result, null, 2));
            setPercent(0);
          }}
        >
          Weather
        </button>
        <div className="meter" style={{ width: "60%" }}>
          <span style={{ width: percent + "%" }}></span>
        </div>
        <input ref={fileInput} type="file" name="largeFile" />
        <button onClick={tusUpload}>Upload</button>
        <div>{data || "No data loaded"}</div>
      </header>
    </div>
  );
}

export default App;
