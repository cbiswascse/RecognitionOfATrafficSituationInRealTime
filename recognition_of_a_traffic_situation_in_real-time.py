from imageai.Detection import VideoObjectDetection
import os
from tkinter import filedialog
import tkinter
from PIL import Image, ImageTk
from tkinter import Tk, RIGHT, BOTH, RAISED
from tkinter.ttk import Frame, Button,Label, Style
import tkinter as tk


class Example(Frame):

    def __init__(self):
        super().__init__()

        self.initUI()


    def initUI(self):

        self.master.title("Recognition of a traffic situation in real-time")
        self.style = Style()
        self.style.theme_use("default")

        frame = Frame(self, relief=RAISED, borderwidth=3)
        frame.pack(fill=BOTH, expand=True)
        bard = Image.open("traffic.jpg")
        bardejov = ImageTk.PhotoImage(bard)
        label1 = Label(self, image=bardejov, borderwidth=3)
        label1.image = bardejov
        label1.place(x=0, y=0)

        self.pack(fill=BOTH, expand=True)

        closeButton = Button(self, text="Close",command=close_window)
        closeButton.pack(side=RIGHT, padx=5, pady=5)
        okButton = Button(self, text="Select file",command = Object_detect)
        okButton.pack(side=RIGHT)

def close_window():
    root.destroy()
    

def forSeconds(second_number, output_arrays, count_arrays, average_output_count):
    print("SECOND : ", second_number)
    print("Array for the outputs of each frame ", output_arrays)
    print("Array for output count for unique objects in each frame : ", count_arrays)
    print("Output average count for unique objects in the last second: ", average_output_count)
    ressult=traffic(count_arrays)
    #print("Precent :", ressult)
    popup = tk.Tk()
    popup.eval('tk::PlaceWindow %s center'% popup.winfo_toplevel())
    if ressult <30:
        T = tk.Text(popup, height=6, width=30)
        T.pack()
        T.insert(tk.END, "\n \t High Traffic\n")
        T.configure(bg='red')
    elif ressult >=30 and ressult < 70:
        T = tk.Text(popup, height=6, width=30)
        T.pack()
        T.insert(tk.END, "\n \t Normal Traffic\n")
        T.configure(bg='yellow')
    elif ressult >= 70 :
        T = tk.Text(popup, height=6, width=30)
        T.pack()
        T.insert(tk.END, "\n \t Low Traffic\n")
        T.configure(bg='green')
    popup.after(30000, lambda: popup.destroy())
    tk.mainloop()
    print("------------END OF A SECOND --------------")


def traffic(count_arrays):
     total_percent=0
     for i in range(len(count_arrays)):
         if i<len(count_arrays)-1:
            if list(count_arrays[i])==list(count_arrays[i+1]) and (len(list(count_arrays[i]))>1 or len(list(count_arrays[i+1]))>1):
                total=0
                objects=list(count_arrays[i])
                for a in objects:
                    if not a == 'person':
                        if a in list(count_arrays[i]):
                            total=total+abs(count_arrays[i+1][a]-count_arrays[i][a])
                    if a == 'person':
                        items=len(objects)-1
                    else:
                        items=len(objects)
                percent=total/items
                percent=int(percent*100)
            else:
                percent=80
         else:
            percent=percent
         total_percent=total_percent+percent
     avg_percent=int((total_percent/len(count_arrays)))
     return avg_percent



def Object_detect():
     filename =  filedialog.askopenfilename(initialdir = "/",title = "Select file",filetypes = (("jpeg files","*.mp4"),("all files","*.*")))
     detector = VideoObjectDetection()
     detector.setModelTypeAsYOLOv3()
     detector.setModelPath( os.path.join(execution_path , "yolo.h5"))
     detector.loadModel()
     
     video_path = detector.detectObjectsFromVideo(input_file_path=filename,
                                                  output_file_path=os.path.join(execution_path, "traffic_mini_detected_1"), 
                                                  frames_per_second=20,
                                                  per_second_function=forSeconds,
                                                  minimum_percentage_probability=30,
                                                  return_detected_frame=False,
                                                  log_progress=True)
     print(video_path)   



root = Tk()
root.geometry("500x480+100+100")

execution_path = os.getcwd()

app = Example()
root.after(30000, lambda: root.destroy())
root.mainloop()
