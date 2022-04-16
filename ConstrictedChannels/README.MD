# ContrictedChannel
ContrictedChannel is an experimental implementation for the following scenario:
There is a finite number a f request processor.
The request processor when finished processing then sends the response as event.
The request processor may receive the message slowly or processes it slowly or both.
The request sender sends the request in a free channel and waits for the response on a task.
After a period the sender times out if no response is sent.
If the response sent after timeout it is thrown away.