"use client";

import React, { useState, useEffect } from "react";
import axios from "axios";

import * as signalR from "@microsoft/signalr";
import { v4 as uuidv4 } from "uuid";
interface Result {
    taskId: number;
    primes: number[];
}

const Home: React.FC = () => {
    const [rangeStart, setRangeStart] = useState(1);
    const [rangeEnd, setRangeEnd] = useState(10);
    const [maxProcessingTime, setMaxProcessingTime] = useState(1);
    const [partialResults, setPartialResults] = useState<Result[]>([]);
    const [finalResult, setFinalResult] = useState<string | null>(null);
    const [isProcessing, setIsProcessing] = useState(false);
    const [bundleId, setBundleId] = useState<string | null>(null);

    // SignalR connection setup
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);

    useEffect(() => {
        const connectToSignalR = async () => {

            const hubConnection = new signalR.HubConnectionBuilder()
                .withUrl(process.env.NEXT_PUBLIC_BACKEND_URL+"/primeHub") 
                .withAutomaticReconnect()
                .build();
            console.log("Hello")
            // Event listeners
            hubConnection.on("ReceivePartialResult", (result) => {
               
                console.log("Received Partial Result:", result);
                setPartialResults((prevResults) => [...prevResults, result]);

            });

            hubConnection.on("ReceiveFinalResult", (finalResult) => {
                console.log("Received Final Result:", finalResult);
                setFinalResult(finalResult.join(", "));
                setIsProcessing(false);
            });

            try {
                await hubConnection.start();
                console.log("SignalR connection established.");

                // Invoke JoinGroup AFTER the connection starts
                await hubConnection.invoke("JoinGroup", bundleId);
                console.log(`Joined group: ${bundleId}`);
            } catch (error) {
                console.error("Error establishing SignalR connection or joining group:", error);
            }

            setConnection(hubConnection);
        };

        if (bundleId) {
            connectToSignalR();
        }

        return () => {
            if (connection) {
                connection.stop();
            }
        };
    }, [bundleId]);

    // Form submission 
    const handleFormSubmit = async () => {
        setIsProcessing(true);
        setPartialResults([]);
        setFinalResult(null);
        const newBundleId = uuidv4();
        setBundleId(newBundleId);

        const requestData = {
            rangeStart,
            rangeEnd,
            bundleId: newBundleId, 
            maxProcessingTime,
        };
        try {
            const response = await axios.post(process.env.NEXT_PUBLIC_BACKEND_URL+"/api/primes", requestData);
            console.log("Task started:", response.data);
        } catch (error) {
            console.error("Error starting the task:", error);
            setIsProcessing(false);
        }
    };

    return (
        <div className="min-h-screen bg-gray-100 flex flex-col items-center py-10">
            {/* Header */}
            <h1 className="text-3xl font-bold text-gray-800 mb-6">Distributed Prime Number Search</h1>

            {/* Input Form */}
            <div className="bg-white shadow-md rounded-lg w-full max-w-xl p-6 mb-6">
                <h2 className="text-xl font-semibold text-gray-700 mb-4">Input Parameters</h2>
                <div className="grid grid-cols-2 gap-4">
                    <div>
                        <label className="block text-sm font-medium text-gray-600 mb-1">Range Start</label>
                        <input
                            type="number"
                            value={rangeStart}
                            onChange={(e) => setRangeStart(parseInt(e.target.value, 10))}
                            className="w-full border-gray-300 text-gray-600 rounded-lg focus:ring focus:ring-blue-200 focus:border-blue-400"
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-medium text-gray-600 mb-1">Range End</label>
                        <input
                            type="number"
                            value={rangeEnd}
                            onChange={(e) => setRangeEnd(parseInt(e.target.value, 10))}
                            className="w-full border-gray-300 rounded-lg text-gray-600 focus:ring focus:ring-blue-200 focus:border-blue-400"
                        />
                    </div>
                    <div className="col-span-2 flex items-center justify-center">
                        <label className="block text-sm font-medium text-gray-600 mb-1">Max Processing Time</label>
                        <input type="range" id="range" name="range" min="1" max="50" value={maxProcessingTime}
                               className="w-64 h-2 bg-gray-300 rounded-lg appearance-none cursor-pointer hover:bg-blue-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
                               onChange={(e) => setMaxProcessingTime(parseInt(e.target.value, 10))}
                        />
                        <span id="rangeValue" className="text-gray-700">{maxProcessingTime}ms</span>
                    </div>
                </div>
                <button
                    onClick={handleFormSubmit}
                    disabled={isProcessing}
                    className={`mt-4 w-full bg-blue-500 text-white py-2 rounded-lg font-medium hover:bg-blue-600 transition ${
                        isProcessing && "opacity-50 cursor-not-allowed"
                    }`}
                >
                    {isProcessing ? "Processing..." : "Start Search"}
                </button>
            </div>

            {/* Results */}
            <div className="bg-white shadow-md rounded-lg w-full max-w-xl p-6">
                <h2 className="text-xl font-semibold text-gray-700 mb-4">Results</h2>

                <div>
                    <h3 className="text-lg font-medium text-gray-800 mb-2">Partial Results:</h3>
                    <ul className="list-disc pl-5 space-y-1 text-gray-600">
                        {partialResults.map((result, index) => (
                            <li key={index}>
                                <strong>({result.primes.length})  {result.taskId}:</strong> {result.primes.join(", ")}
                            </li>
                        ))}
                    </ul>
                    {isProcessing ? (
                        <p className="text-center text-gray-500">Processing... Please wait.</p>
                    ) : (
                        finalResult && (
                            <p className="mt-4 text-lg font-medium text-green-600">
                                Final Result: <span className="font-normal">{finalResult}</span>
                            </p>
                        )
                    )}
                </div>
            </div>
        </div>
    );

};

export default Home;