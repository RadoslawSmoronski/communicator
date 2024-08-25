const requestOptions = {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                userName: this.state.username,
                password: this.state.password
            })
        };


        fetch('http://localhost:5205/api/user/register', requestOptions)
        .then(async response => {
            const isJson = response.headers.get('content-type')?.includes('application/json');
            const data = isJson && await response.json();

            // check for error response
            if (!response.ok) {
                // get error message from body or default to response status
                const error = (data && data.message) || response.status;
                return Promise.reject(error);
            }

            //is ok
            if(data.succeeded){
                console.log(data);
                this.showPopUpMess(data.message);
            }
            
        })
        .catch(error => {
            this.showPopUpMess(error.toString());
        });