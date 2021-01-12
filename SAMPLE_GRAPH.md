# Sample Graph

When using the ```Test``` application, you can issue the command ```default``` to load a sample graph.  It will load the following.

## People
```
{'guid':'joel','type':'person','first':'Joel','city':'San Jose'}
{'guid':'maria','type':'person','first':'Maria','city':'San Jose'}
{'guid':'jason','type':'person','first':'Jason','city':'San Jose'}
{'guid':'scott','type':'person','first':'Scott','city':'Chicago'}
{'guid':'may','type':'person','first':'May','city':'New York City'}
{'guid':'matt','type':'person','first':'Matt','city':'Raleigh'}
{'guid':'bob','type':'person','first':'Bob','city':'Asheville'}
```
## Things
```
{'guid':'car1','type':'car','make':'Toyota','model':'Highlander'}
{'guid':'car2','type':'car','make':'Volkswagen','model':'Jetta'}
{'guid':'car3','type':'car','make':'Mercedes','model':'SUV'}
{'guid':'guitar','type':'instrument','make':'Jackson','model':'Soloist'}
{'guid':'piano','type':'instrument','make':'Yamaha','model':'Keyboard'}
{'guid':'house','type':'house','desc':'Super duper house'}
```
## Relationships
```
{'guid':'r1','type':'lives_in','data':'foo'}      -> joel -> house
{'guid':'r2','type':'lives_in','data':'bar'}      -> maria -> house
{'guid':'r3','type':'lives_in','data':'baz'}      -> jason -> house
{'guid':'r4','type':'friends_with','data':'foo'}  -> joel -> scott
{'guid':'r5','type':'friends_with','data':'bar'}  -> maria -> may
{'guid':'r6','type':'worked_with','data':'baz'}   -> joel -> matt
{'guid':'r7','type':'worked_with','data':'foo'}   -> matt -> bob
{'guid':'r8','type':'is_child_of','data':'bar'}   -> jason -> maria
```
