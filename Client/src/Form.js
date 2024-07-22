import React from "react";
import Note from "./Note";

class Form extends React.Component{
    constructor(props) {
        super(props);
        this.state = {text: '', title: '',
            data: [
                {title: "lista zakupów", text: "1. jabłko\n2. marchew"},
                {title: "terminy", text: "1.01.2023\n5.02.2024"},
                {title: "plan dnia", text: "1. Spotkanie\n2. Obiad"},
                {title: "notatki", text: "Przypomnieć o urodzinach mamy\nKupić bilety"},
                {title: "zadania", text: "1. Praca domowa\n2. Siłownia"},
                {title: "menu", text: "Śniadanie: Jajecznica\nObiad: Kurczak z ryżem"},
                {title: "ćwiczenia", text: "1. Bieganie\n2. Joga"},
                {title: "cele", text: "Nauczyć się nowego języka\nPoprawić kondycję"},
                {title: "projekty", text: "Zakończyć raport\nZacząć nowy projekt"},
                {title: "wydarzenia", text: "Koncert: 12.03.2024\nWystawa: 20.04.2024"},
                {title: "przepisy", text: "1. Zupa pomidorowa\n2. Lasagna"},
                {title: "spotkania", text: "Spotkanie z klientem: 3.05.2024\nSpotkanie zespołu: 10.05.2024"},
                {title: "lektury", text: "1. 'Wojna i pokój'\n2. 'Zbrodnia i kara'"},
                {title: "filmy", text: "1. 'Incepcja'\n2. 'Matrix'"},
                {title: "plany podróży", text: "1. Hiszpania\n2. Włochy"},
                {title: "urodziny", text: "1. Ania: 10.06.2024\n2. Piotr: 25.07.2024"},
                {title: "zakupy", text: "1. Mleko\n2. Chleb"},
                {title: "koszyk zakupowy", text: "1. Laptop\n2. Słuchawki"},
                {title: "listy", text: "1. Do zrobienia\n2. Do kupienia"},
                {title: "plany weekendowe", text: "1. Wycieczka do lasu\n2. Wizyta u przyjaciół"}
            ]
        };
    
        this.handleChange = this.handleChange.bind(this);
        this.addNote = this.addNote.bind(this);
    }


    addNote(){
        console.log(this.state.text + " " + this.state.title);

        let newData = this.state.data;
        newData.push({title: this.state.title, text: this.state.text})

        this.setState({data: newData, text: '', title: ''});
    }

    handleChange(event){
        let titleVal = document.getElementById("ftitle").value;
        let textVal = document.getElementById("ftext").value;
        this.setState({text: textVal, title: titleVal});
    }

    render(){


        

        let notes = [];
        for(let i=0; i < this.state.data.length; i++){
            notes.push(<Note title={this.state.data[i].title} text={this.state.data[i].text} key={i}/>)
        }
        return(
            <>
            <div>
                <label htmlFor="ftitle">Title: </label>
                <input name="ftitle" id="ftitle" type="text" value={this.state.title} onChange={this.handleChange}/>
                <label htmlFor="ftext">Text: </label>
                <input name="ftext" id="ftext" type="text" value={this.state.text} onChange={this.handleChange}/>
                <button onClick={this.addNote}>Dodaj notatkę</button>
            </div>
            <div id="notes">
                {notes}
            </div>
            </>
        );
    }

}
export default Form;